using System.Linq;

namespace kenzauros.RHarbor
{
    internal static class FingerPrintUtil
    {
        public static string ToFingerPrintString(this byte[] fingerprint)
        {
            return string.Join(":", fingerprint.Select(x => x.ToString("x2")));
        }
    }
}
