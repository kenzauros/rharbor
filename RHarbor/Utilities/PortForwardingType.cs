using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kenzauros.RHarbor
{
    internal enum PortForwardingType
    {
        Local,
        Remote,
        Dynamic,
    }

    internal static class PortForwardingTypeExtensions
    {
        private static string[] _Names;
        public static string[] Names => _Names ?? (_Names = Enum.GetNames(typeof(PortForwardingType)));
    }
}
