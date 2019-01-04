using kenzauros.RHarbor.Models;
using Reactive.Bindings;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace kenzauros.RHarbor.ViewModels
{
    internal class RDPConnectionViewModel : ConnectionViewModel<RDPConnectionInfo>
    {
        /// <summary>
        /// Internal <see cref="RDPClient"/> instance created by <see cref="Connect"/>.
        /// </summary>
        public RDPClient RDPClient { get; private set; }

        #region Constructor

        /// <summary>
        /// Constructor for view sample.
        /// </summary>
        public RDPConnectionViewModel() : base() { }

        public RDPConnectionViewModel(RDPConnectionInfo info) : base(info)
        {
            PrepareRequiredConnection(info.RequiredConnection);
        }

        #endregion

        #region Connect

        /// <summary>
        /// Starts to connect to the host.
        /// </summary>
        /// <returns></returns>
        public override async Task Connect()
        {
            IsConnecting.Value = true;
            IsConnected.Value = false;
            await EstablishRequiredConnection();
            RDPClient = new RDPClient(ConnectionInfo);
            RDPClient.Closed += (s, e) =>
            {
                App.Current.Dispatcher.Invoke(async () =>
                {
                    await Disconnect();
                });
            };
            var (host, port) = GetActualEndPoint();
            this.WriteLog("Connecting...");
            await RDPClient.Open(host, port);
            this.WriteLog("Connection Established.");
            IsConnected.Value = RDPClient.IsOpened;
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
            this.WriteLog("Disconnecting...");
            if (RDPClient?.IsOpened == true)
            {
                await RDPClient.Close();
            }
            await DisconnectRequiredConnection();
            this.WriteLog("Disconnected.");
            IsConnecting.Value = false;
            IsConnected.Value = false;
        }

        /// <summary>
        /// Dispose the resources.
        /// </summary>
        public override async void Dispose()
        {
            await Disconnect();
            base.Dispose();
        }

        #endregion

    }
}
