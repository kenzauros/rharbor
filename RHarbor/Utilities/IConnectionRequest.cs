using kenzauros.RHarbor.Models;
using System.Collections.Generic;

namespace kenzauros.RHarbor.Utilities
{
    internal interface IConnectionRequest
    {
        /// <summary>
        /// Queue of requested connections
        /// </summary>
        Queue<(ConnectionType Type, int ConnectionId)> Queue { get; }
    }

    internal class ConnectionRequest : IConnectionRequest
    {
        private static ConnectionRequest _Singleton;
        public static ConnectionRequest Singleton => _Singleton ?? (_Singleton = new ConnectionRequest());

        private ConnectionRequest() { }

        /// <summary>
        /// Queue of requested connections
        /// </summary>
        public Queue<(ConnectionType Type, int ConnectionId)> Queue { get; }
            = new Queue<(ConnectionType Type, int ConnectionId)>();
    }
}
