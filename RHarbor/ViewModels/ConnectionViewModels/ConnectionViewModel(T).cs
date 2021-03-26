using kenzauros.RHarbor.Models;
using kenzauros.RHarbor.Properties;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Renci.SshNet;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace kenzauros.RHarbor.ViewModels
{
    internal class ConnectionViewModel<T> : ConnectionViewModel where T : class, IConnectionInfo
    {
        /// <summary>
        /// Information for the connection.
        /// </summary>
        public T ConnectionInfo { get; set; }

        #region Create

        public static ConnectionViewModel<T> CreateFromConnectionInfo(T info)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));

            ConnectionViewModel<T> vm;
            if (info is RDPConnectionInfo)
            {
                vm = new RDPConnectionViewModel(info as RDPConnectionInfo) as ConnectionViewModel<T>;
            }
            else if (info is SSHConnectionInfo)
            {
                vm = new SSHConnectionViewModel(info as SSHConnectionInfo) as ConnectionViewModel<T>;
            }
            else
            {
                throw new ArgumentException($"{info.GetType()} is not available connection info.", nameof(info));
            }
            vm.IsRoot.Value = true;
            return vm;
        }

        #endregion

        #region Constructor

        public ConnectionViewModel()
        {
            DisconnectCommand = IsConnected.CombineLatest(IsConnecting, (a, b) => a || b).ToAsyncReactiveCommand();
            DisconnectCommand.Subscribe(() => Disconnect()).AddTo(Disposable);

            DisplayText = ConnectionName.CombineLatest(Host, Port, IsConnecting,
                (name, host, port, isConnecting) =>
                {
                    var hostport = host + (port == null ? "" : $":{port}");
                    var status = "";
                    if (isConnecting) status = $" - {Resources.Connection_ConnectingText}";
                    var identity = string.IsNullOrWhiteSpace(name)
                        ? hostport
                        : $"{name} ({hostport})";
                    return identity + status;
                }).ToReactiveProperty();
        }

        public ConnectionViewModel(T info) : this()
        {
            ConnectionInfo = info ?? throw new ArgumentException($"Connection info cannot be null.", nameof(info));
            ConnectionName.Value = info.Name;
            Host.Value = info.Host;
            Port.Value = info.Port;
        }

        #endregion

        #region Required connection

        protected SSHConnectionViewModel RequiredConnection { get; set; }

        protected void PrepareRequiredConnection(SSHConnectionInfo requiredConnectionInfo)
        {
            if (requiredConnectionInfo == null) return;
            var additionalPF = new PortForwarding
            {
                Type = "Local",
                RemoteHost = ConnectionInfo.Host,
                RemotePort = ConnectionInfo.Port,
            };
            // Try to re-use recently used port.
            var recentlyUsedPort = ConnectionInfo.GetAvailableCachedPort();
            if (recentlyUsedPort > 0)
            {
                additionalPF.LocalPort = recentlyUsedPort;
            }
            var conn = new SSHConnectionViewModel(requiredConnectionInfo, true, additionalPF)
            {
                Parent = this
            };
            conn.IsRoot.Value = false;
            RequiredConnection = conn;
            Children.Add(conn);
        }

        protected (string host, int port) GetActualEndPoint()
        {
            var host = ConnectionInfo.Host;
            var port = ConnectionInfo.Port;
            if (RequiredConnection != null)
            {
                // Use required connection's port forwarding
                var pf = RequiredConnection.ForwardedPorts
                    .Select(x => x.ForwardedPort.Value as ForwardedPortLocal)
                    .FirstOrDefault(x => x?.IsStarted == true && x.Host == ConnectionInfo.Host)
                    ?? throw new Exception($"Port forwarding for {ConnectionInfo.Host} has not be established.");
                return (pf.BoundHost, (int)pf.BoundPort);
            }
            return (host, port);
        }

        /// <summary>
        /// Establish the required connections if needed.
        /// </summary>
        /// <returns></returns>
        protected async Task EstablishRequiredConnection()
        {
            if (RequiredConnection == null) return;
            this.WriteLog($"Connection \"{RequiredConnection.ToString()}\" Required.");
            await RequiredConnection.Connect();
            ConnectionInfo.AddPortCache(GetActualEndPoint().port); // Cache the current forwarding port number
        }

        /// <summary>
        /// Disconnect the required connections if needed.
        /// </summary>
        /// <returns></returns>
        protected async Task DisconnectRequiredConnection()
        {
            if (RequiredConnection == null) return;
            await RequiredConnection.Disconnect();
            Children.Remove(RequiredConnection);
        }

        #endregion

        #region ToString

        /// <summary>
        /// Returns the text describes the connection info.
        /// Used in Logging in <see cref="ConnectionViewModel.this.WriteLog(string)"/> method.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ConnectionInfo.ToString();
        }

        #endregion
    }
}
