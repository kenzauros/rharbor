using System.Collections.Generic;

namespace kenzauros.RHarbor.Rpc
{
    public class ConnectionRequest
    {
        /// <summary>
        /// Queue of requested connections
        /// </summary>
        public static Queue<(string Type, int ConnectionId)> Queue { get; } = new Queue<(string Type, int ConnectionId)>();
    }
}
