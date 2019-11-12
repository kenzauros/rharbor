using System.ServiceModel;

namespace kenzauros.RHarbor.IPC
{
    /// <summary>
    /// Service class for IPC
    /// </summary>
    static class IPCService
    {
        private const string Url = "net.pipe://localhost/process-commander";

        /// <summary>
        /// Creates and opens a service host of the ProcessCommander.
        /// </summary>
        /// <returns></returns>
        public static ServiceHost OpenServiceHost(ProcessCommander singleton)
        {
            var serviceHost = new ServiceHost(singleton);
            serviceHost.AddServiceEndpoint(
                typeof(IProcessCommander),
                new NetNamedPipeBinding(),
                Url
                );
            serviceHost.Open();
            return serviceHost;
        }

        /// <summary>
        /// Creates a client service channel to send a command to the service host.
        /// </summary>
        /// <returns></returns>
        public static IProcessCommander CreateServiceChannel()
            => new ChannelFactory<IProcessCommander>(
                    new NetNamedPipeBinding(),
                    Url
                    ).CreateChannel();
    }
}
