﻿using kenzauros.RHarbor.Models;
using kenzauros.RHarbor.MvvmDialog;
using kenzauros.RHarbor.Properties;
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
        /// <param name="asSubordinate">Whether invoked from another connection or not.</param>
        /// <param name="additionalPortForwarding"><see cref="PortForwarding"/> which will be additionally established.</param>
        public SSHConnectionViewModel(SSHConnectionInfo info, bool asSubordinate = false, PortForwarding additionalPortForwarding = null) : base(info)
        {
            if (string.IsNullOrWhiteSpace(info.Username))
            {
                throw new InvalidOperationException(Resources.SSHConnection_Exception_UsernameNotAssigned);
            }
            if (!string.IsNullOrWhiteSpace(info.PrivateKeyFilePath) && !File.Exists(info.PrivateKeyFilePath))
            {
                throw new FileNotFoundException(
                    string.Format(Resources.SSHConnection_Exception_PrivateKeyFileNotExists, info.PrivateKeyFilePath),
                    info.PrivateKeyFilePath);
            }
            if (info.PortForwardingCollection != null
                && ((info.AlwaysForwardPorts ?? false) || !asSubordinate))
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
                Children.Add(new ForwardedPortConnectionViewModel(CreateForwardedPort(pf), pf.Name));
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
                    throw new InvalidOperationException(Resources.SSHConnection_Exception_RemotePortNotAssigned);
                }
                if (!pf.RemotePort.HasValue || !pf.RemotePort.Value.IsValidIPPort()
                    || !pf.LocalPort.HasValue || !pf.LocalPort.Value.IsValidIPPort())
                {
                    throw new InvalidOperationException(Resources.SSHConnection_Exception_PortNumberOutOfRange);
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
                    throw new InvalidOperationException(Resources.SSHConnection_Exception_PortNumberOutOfRange);
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
                    throw new InvalidOperationException(Resources.SSHConnection_Exception_RemoteHostPortNotAssigned);
                }
                if (!pf.RemotePort.Value.IsValidIPPort() ||
                    (pf.LocalPort.HasValue && !pf.LocalPort.Value.IsValidIPPort()))
                {
                    throw new InvalidOperationException(Resources.SSHConnection_Exception_PortNumberOutOfRange);
                }
                var boundHost = string.IsNullOrWhiteSpace(pf.LocalHost) ? "127.0.0.1" : pf.LocalHost;
                return (pf.LocalPort == null)
                    ? new ForwardedPortLocal(boundHost, pf.RemoteHost, (uint)pf.RemotePort.Value)
                    : new ForwardedPortLocal(boundHost, (uint)pf.LocalPort.Value, pf.RemoteHost, (uint)pf.RemotePort.Value);
            }
        }

        #endregion

        #region Connect

        private AuthenticationMethod[] GetAuthMethods(string username, string password)
        {
            var authMethods = new List<AuthenticationMethod>();
            if (string.IsNullOrWhiteSpace(ConnectionInfo.PrivateKeyFilePath))
            {
                // Password Auth
                var auth = new PasswordAuthenticationMethod(username, password ?? "");
                authMethods.Add(auth);
            }
            else
            {
                // Private key auth
                var pkeyfile = new PrivateKeyFile(ConnectionInfo.PrivateKeyFilePath, password ?? "");
                var auth = new PrivateKeyAuthenticationMethod(username, pkeyfile);
                authMethods.Add(auth);
            }
            return authMethods.ToArray();
        }

        private ConnectionInfo CreateSshConnectionInfo(string username, string password)
        {
            var (host, port) = GetActualEndPoint();
            var authMethods = GetAuthMethods(username, password);
            return new ConnectionInfo(host, port, username, authMethods);
        }

        private async Task<(string username, string password, bool needSavePassword, bool passwordChanged)> GetUsernameAndPassword(bool forceInput, string message = null)
        {
            var username = ConnectionInfo.Username;
            var password = ConnectionInfo.RawPassword;
            var savePassword = ConnectionInfo.SavePassword;
            bool passwordChanged = false;
            if (forceInput || username == null || password == null)
            {
                bool result;
                (result, username, password, savePassword) = await MainWindow.ShowAuthenticationDialog(
                    username: username,
                    savePassword: savePassword,
                    title: Resources.SSHConnection_Dialog_Auth_Title,
                    message: message
                    );
                // User cancel
                if (!result) throw new OperationCanceledException();
                passwordChanged = true;
            }
            return (username, password, savePassword, passwordChanged);
        }

        private async Task TryEstablishConnection(string username, string password)
        {
            var client = new SshClient(CreateSshConnectionInfo(username, password))
            {
                KeepAliveInterval = TimeSpan.FromMilliseconds(ConnectionInfo.KeepAliveInterval)
            };
            const int DefaultConnectionTimeout = 5000;
            client.ConnectionInfo.Timeout = TimeSpan.FromMilliseconds(ConnectionInfo.ConnectionTimeout ?? DefaultConnectionTimeout);
            var ev = new ManualResetEventSlim(false);
            void hostKeyEventHandler(object o, HostKeyEventArgs e)
            {
                if (e.FingerPrint == null) throw new Exception(Resources.SSHConnection_Exception_FingerPrintIsEmpty);
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
                    e.CanTrust = await MainWindow.ShowConfirmationDialog(
                        $"{Resources.SSHConnection_Dialog_FingerPrintValidation_Messsage}\n{fingerprint}", Resources.SSHConnection_Dialog_FingerPrintValidation_Title);
                    if (e.CanTrust)
                    {
                        // Save fingerprint
                        await MainWindow.DbContext.UpdateSSHFingerPrint(ConnectionInfo, fingerprint);
                        this.WriteLog("Finger Print Mathed and Successfully Saved.");
                    }
                    else
                    {
                        this.WriteLog("Finger Print Rejected.");
                    }
                    ev.Set();
                });
            }
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

        private async Task EstablishForwardedPorts()
        {
            foreach (var fp in ForwardedPorts)
            {
                try
                {
                    SshClient.AddForwardedPort(fp.ForwardedPort.Value);
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
            await EstablishRequiredConnection();
            string username;
            string password;
            bool savePassword;
            bool passwordChanged;
            bool forceInput = false;
            string additionalMessage = "";
            while (true) // Try to establish this connection until connected.
            {
                (username, password, savePassword, passwordChanged)
                    = await GetUsernameAndPassword(
                        forceInput,
                        $"{string.Format(Resources.SSHConnection_Dialog_Auth_Message, ToString())}\n{additionalMessage}");
                additionalMessage = "";
                try
                {
                    await TryEstablishConnection(username, password);
                }
                catch (SshAuthenticationException ex) // Authentication Failed.
                {
                    // Reauth
                    additionalMessage = $"{ex.Message}";
                    forceInput = true; // Force showing auth dialog.
                    continue;
                }
                catch (Exception) // Other exception.
                {
                    throw;
                }
                if (passwordChanged)
                {
                    await MainWindow.DbContext.SavePassword(ConnectionInfo, savePassword, password);
                    this.WriteLog($"Password Saved.");
                }
                break; // Connection established.
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
            if (SshClient == null) return;
            this.WriteLog("Disconnecting...");
            var ports = ForwardedPorts?.ToList();
            if (ports?.Count > 0)
            {
                foreach (var pf in ports)
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
                ports.ForEach(x => Children.Remove(x));
                this.WriteLog("Closed All Forwarded Ports.");
            }
            if (SshClient != null)
            {
                try
                {
                    await Task.Run(() =>
                    {
                        SshClient?.Disconnect();
                        SshClient?.Dispose();
                        SshClient = null;
                    });
                    this.WriteLog("Disconnected.");
                }
                catch (Exception ex)
                {
                    this.WriteLog("Failed to disconnect or dispose.", ex);
                }
            }
            await DisconnectRequiredConnection();
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
