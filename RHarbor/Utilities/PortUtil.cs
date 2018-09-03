using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kenzauros.RHarbor
{
    internal static class PortUtil
    {
        /// <summary>
        /// Returns if the number is valid for IP port.
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public static bool IsValidIPPort(this int port)
        {
            return (port > 0 && port < 65536);
        }
    }
}
