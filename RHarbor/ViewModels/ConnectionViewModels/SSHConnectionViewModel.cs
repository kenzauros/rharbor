using kenzauros.RHarbor.Models;
using kenzauros.RHarbor.MvvmDialog;
using Reactive.Bindings;
using Renci.SshNet;
using Renci.SshNet.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

namespace kenzauros.RHarbor.ViewModels
{
    internal class SSHConnectionViewModel : ConnectionViewModel<SSHConnectionInfo>
    {
        /// <summary>
        /// Internal <see cref="SshClient"/> instance created by <see cref="Connect"/>.
        /// </summary>
        public SshClient SshClient { get; private set; }

        #region Constructor

        /// <summary>
        /// Constructor for view sample.
        /// </summary>
        public SSHConnectionViewModel() : base() { }

        /// <summary>
        /// Instantiates <see cref="SSHConnectionViewModel"/> from <see cref="SSHConnectionInfo"/> and returns the root connection.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="createRegisteredForwardedPorts"></param>
        public SSHConnectionViewModel(SSHConnectionInfo info, bool createRegisteredForwardedPorts = true, PortForwarding additionalPortForwarding = null) : base(info)
        {
            if (string.IsNullOrWhiteSpace(info.Username))
            {
                throw new InvalidOperationException("Username should be assigned.");
            }
            if (!string.IsNullOrWhiteSpace(info.PrivateKeyFilePath) && !File.Exists(info.PrivateKeyFilePath))
            {
                throw new FileNotFoundException("Private key file does not exist.", info.PrivateKeyFilePath);
            }
            if (createRegisteredForwardedPorts && info.PortForwardingCollection != null)
            {
                CreateForwardedPorts(info.PortForwardingCollection);
            }
            if (additionalPortForwarding != null)
            {
                CreateForwardedPorts(new[] { additionalPortForwarding });
            }
            PrepareRequiredConnection(info.RequiredConnection);
        }

        #endregion

        #region ForwardedPorts

        /// <summary>
        /// Enumerate view models with <see cref="ForwardedPort"/> in this <see cref="ConnectionViewModel.Children"/>.
        /// </summary>
        public IEnumerable<ForwardedPortConnectionViewModel> ForwardedPorts
            => Children
                .Where(x => x is ForwardedPortConnectionViewModel)
                .Select(x => x as ForwardedPortConnectionViewModel);

        /// <summary>
        /// Create <see cref="ForwardedPort"/>s from the collection of <see cref="PortForwarding"/>.
        /// </summary>
        /// <param name="portForwardings"></param>
        private void CreateForwardedPorts(ICollection<PortForwarding> portForwardings)
        {
            foreach (var pf in portForwardings)
            {
                Children.Add(new ForwardedPortConnectionViewModel(CreateForwardedPort(pf)));
            }
        }

        /// <summary>
        /// Create a <see cref="ForwardedPort"/> from <see cref="PortForwarding"/> setting.
        /// </summary>
        /// <param name="pf"></param>
        /// <returns></returns>
        private ForwardedPort CreateForwardedPort(PortForwarding pf)
        {
            if (pf.Type == "Remote")
            {
                if (pf.RemotePort == null)
                {
                    throw new InvalidOperationException("Remote Port should be assigned.");
                }
                if (!pf.RemotePort.HasValue || !pf.RemotePort.Value.IsValidIPPort()
                    || !pf.LocalPort.HasValue || !pf.LocalPort.Value.IsValidIPPort())
                {
                    throw new InvalidOperationException("Port number should be between 1 and 65535.");
                }
                // LocalHost and LocalPort are the information of the SSH server.
                return (string.IsNullOrWhiteSpace(pf.LocalHost))
                    ? new ForwardedPortRemote((uint)pf.LocalPort, pf.RemoteHost, (uint)pf.RemotePort)
                    : new ForwardedPortRemote(pf.LocalHost, (uint)pf.LocalPort, pf.RemoteHost, (uint)pf.RemotePort);
            }
            else if (pf.Type == "Dynamic")
            {
                // RemoteHost and RemotePort don't care.
                if (!pf.LocalPort.HasValue || !pf.LocalPort.Value.IsValidIPPort())
                {
                    throw new InvalidOperationException("Port number should be between 1 and 65535.");
                }
                var boundHost = string.IsNullOrWhiteSpace(pf.LocalHost) ? "127.0.0.1" : pf.LocalHost;
                return (string.IsNullOrWhiteSpace(pf.LocalHost))
                    ? new ForwardedPortDynamic(boundHost, (uint)pf.LocalPort.Value)
                    : new ForwardedPortDynamic((uint)pf.LocalPort.Value);
            }
            else // Type not assgined / Type == "Local"
            {
                if (string.IsNullOrWhiteSpace(pf.RemoteHost) || pf.RemotePort == null)
                {
                    throw new InvalidOperationException("Remote Host and Port should be assigned.");
                }
                if (!pf.RemotePort.Value.IsValidIPPort() ||
                    (pf.LocalPort.HasValue && !pf.LocalPort.Value.IsValidIPPort()))
                {
                    throw new InvalidOperationException("Port number should be between 1 and 65535.");
                }
                var boundHost = string.IsNullOrWhiteSpace(pf.LocalHost) ? "127.0.0.1" : pf.LocalHost;
                return (pf.LocalPort == null)
                    ? new ForwardedPortLocal(boundHost, pf.RemoteHost, (uint)pf.RemotePort.Value)
                    : new ForwardedPortLocal(boundHost, (uint)pf.LocalPort.Value, pf.RemoteHost, (uint)pf.RemotePort.Value);
            }
        }

        #endregion

        #region Connect

        private AuthenticationMethod[] GetAuthMethods(string username, SecureString password)
        {
            var authMethods = new List<AuthenticationMethod>();
            if (string.IsNullOrWhiteSpace(ConnectionInfo.PrivateKeyFilePath))
            {
                // Password Auth
                var auth = new PasswordAuthenticationMethod(username, password?.GetPlainString() ?? "");
                authMethods.Add(auth);
            }
            else
            {
                // Private key auth
                var pkeyfile = new PrivateKeyFile(ConnectionInfo.PrivateKeyFilePath, password?.GetPlainString() ?? "");
                var auth = new PrivateKeyAuthenticationMethod(username, pkeyfile);
                authMethods.Add(auth);
            }
            return authMethods.ToArray();
        }

        private ConnectionInfo CreateSshConnectionInfo(string username, SecureString password)
        {
            var (host, port) = GetActualEndPoint();
            var authMethods = GetAuthMethods(username, password);
            return new ConnectionInfo(host, port, username, authMethods);
        }

        private async Task<(string username, SecureString password, bool needSavePassword, bool passwordChanged)> GetUsernameAndPassword(bool forceInput)
        {
            var username = ConnectionInfo.Username;
            var password = ConnectionInfo.SecurePassword;
            var savePassword = ConnectionInfo.SavePassword;
            bool passwordChanged = false;
            if (forceInput || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password?.GetPlainString()))
            {
                bool result;
                (result, username, password, savePassword) = await MainWindow.ShowAuthenticationDialog(
                    username: username,
                    savePassword: savePassword
                    );
                // User cancel
                if (!result) throw new OperationCanceledException();
                passwordChanged = true;
            }
            return (username, password, savePassword, passwordChanged);
        }

        private async Task<bool> TryEstablishConnection(string username, SecureString password)
        {
            var client = new SshClient(CreateSshConnectionInfo(username, password))
            {
                KeepAliveInterval = TimeSpan.FromMilliseconds(ConnectionInfo.KeepAliveInterval)
            };
            var ev = new ManualResetEventSlim(false);
            void hostKeyEventHandler(object o, HostKeyEventArgs e)
            {
                if (e.FingerPrint == null) throw new Exception("Empty finger print received.");
                var fingerprint = e.FingerPrint?.ToFingerPrintString();
                if (ConnectionInfo.ExpectedFingerPrint == fingerprint)
                {
                    // Fingerprint matched
                    e.CanTrust = true;
                    this.WriteLog("Finger Print Matched.");
                    ev.Set();
                    return;
                }
                App.Current.Dispatcher.Invoke(async () =>
                {
                    var truested = await MainWindow.ShowConfirmationDialog(
                        $"Please check the finger print of the SSH Server.\n{fingerprint}", "SSH FingerPrint");
                    if (truested)
                    {
                        e.CanTrust = true;
                        // Save fingerprint
                        await MainWindow.DbContext.UpdateSSHFingerPrint(ConnectionInfo, fingerprint);
                        this.WriteLog("Finger Print Mathed and Successfully Saved.");
                        ev.Set();
                    }
                    else
                    {
                        e.CanTrust = false;
                        this.WriteLog("Finger Print Rejected.");
                        ev.Set();
                    }
                });
            }
            try
            {
                await Task.Run(() =>
                {
                    client.HostKeyReceived += hostKeyEventHandler;
                    this.WriteLog("Connecting...");
                    client.Connect();
                    ev.Wait(); // Wait for checking out the finger print
                    this.WriteLog("Connected...");
                    client.HostKeyReceived -= hostKeyEventHandler;
                });
                SshClient = client;
            }
            catch (Exception ex)
            {
                MyLogger.Log("Connection Failed.", ex);
                return false;
            }
            return true;
        }

        private async Task EstablishForwardedPorts()
        {
            foreach (var fp in ForwardedPorts)
            {
                try
                {
                    SshClient.AddForwardedPort(fp.ForwardedPort);
                    await fp.Connect();
                    this.WriteLog($"Port Forwarding Established ({fp.ToString()}).");
                }
                catch (Exception ex)
                {
                    this.WriteLog($"Failed to Establish Port Forwarding: ({fp.ToString()}).", ex);
                }
            }
        }

        /// <summary>
        /// Starts to connect to the host.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="OperationCanceledException">User has canceled the operation in authentication.</exception>
        public override async Task Connect()
        {
            IsConnecting.Value = true;
            IsConnected.Value = false;
            this.WriteLog($"Connecting...");
            // Establish the required connections.
            if (RequiredConnection != null)
            {
                this.WriteLog($"Connection \"{RequiredConnection.ToString()}\" Required.");
                await RequiredConnection.Connect();
            }
            // Try to establish this connection until connected.
            string username;
            SecureString password;
            bool savePassword;
            bool passwordChanged;
            bool forceInput = false;
            while (true)
            {
                (username, password, savePassword, passwordChanged) = await GetUsernameAndPassword(forceInput);
                if (await TryEstablishConnection(username, password))
                {
                    if (passwordChanged)
                    {
                        await MainWindow.DbContext.SavePassword(ConnectionInfo, savePassword, password);
                        this.WriteLog($"Password Saved.");
                    }
                    break; // Connection established.
                }
                forceInput = true; // Force showing auth dialog.
            }

            if (SshClient.IsConnected)
            {
                this.WriteLog("Connection Established.");
                await EstablishForwardedPorts();
            }
            else
            {
                this.WriteLog("Connection Failed.");
            }
            IsConnected.Value = SshClient.IsConnected;
            IsConnecting.Value = false;
        }

        #endregion

        #region Disconnect/Dispose

        /// <summary>
        /// Disconnects.
        /// </summary>
        /// <returns></returns>
        public override async Task Disconnect()
        {
            if (RequiredConnection != null)
            {
                await RequiredConnection.Disconnect();
                Children.Remove(RequiredConnection);
            }
            if (SshClient != null)
            {
                this.WriteLog("Disconnecting...");
                if (ForwardedPorts?.Any() == true)
                {
                    foreach (var pf in ForwardedPorts)
                    {
                        try
                        {
                            await pf.Disconnect();
                        }
                        catch (Exception ex)
                        {
                            MyLogger.Log("Failed to close a forwarded port.", ex);
                        }
                    }
                    ForwardedPorts.ToList().ForEach(x => Children.Remove(x));
                    this.WriteLog("Closed All Forwarded Ports.");
                }
                try
                {
                    await Task.Run(() =>
                    {
                        SshClient.Disconnect();
                        SshClient.Dispose();
                        SshClient = null;
                    });
                    this.WriteLog("Disconnected.");
                }
                catch (Exception ex)
                {
                    this.WriteLog("Failed to disconnect or dispose.", ex);
                }
            }
            IsConnecting.Value = false;
            IsConnected.Value = false;
        }

        /// <summary>
        /// Dispose the resources.
        /// </summary>
        public override async void Dispose()
        {
            await Disconnect().ConfigureAwait(false);
            base.Dispose();
        }

        #endregion

    }
}
