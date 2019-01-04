using kenzauros.RHarbor.Properties;
using System.Resources;

namespace kenzauros.RHarbor
{
    internal static class LocalizedResources
    {
        readonly static ResourceManager _ResourceManager = new ResourceManager(typeof(Resources));

        public static string GetString(string resourceKey)
        {
            return _ResourceManager.GetString(resourceKey) ?? resourceKey;
        }
    }
}
