using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace kenzauros.RHarbor.RDP
{
    /// <summary>
    /// Locates FreeRDP executable files in the application directory.
    /// </summary>
    internal static class FreeRDPLocator
    {
        /// <summary>
        /// FreeRDP executable file names in priority order.
        /// sdl-freerdp.exe is preferred over wfreerdp.exe.
        /// </summary>
        internal static readonly string[] ExeNames = { "sdl-freerdp.exe", "wfreerdp.exe" };

        /// <summary>
        /// Returns the candidate FreeRDP executable paths in priority order.
        /// Searches <c>freerdp*</c> subdirectories of <paramref name="baseDirectory"/>,
        /// sorted by folder name descending so that the newest version is tried first.
        /// Within each folder, <c>sdl-freerdp.exe</c> is preferred over <c>wfreerdp.exe</c>.
        /// Returns an empty sequence when no FreeRDP installation is found.
        /// </summary>
        /// <param name="baseDirectory">
        /// Directory to search. Defaults to <see cref="AppDomain.CurrentDomain.BaseDirectory"/>
        /// when <c>null</c>.
        /// </param>
        public static IEnumerable<string> FindExecutables(string baseDirectory = null)
        {
            var appDir = baseDirectory ?? System.AppDomain.CurrentDomain.BaseDirectory;
            if (!Directory.Exists(appDir))
                yield break;

            // Find all freerdp* directories sorted by name descending (newest version first)
            var freerdpDirs = Directory.GetDirectories(appDir, "freerdp*")
                .OrderByDescending(d => Path.GetFileName(d));

            foreach (var dir in freerdpDirs)
            {
                foreach (var exeName in ExeNames)
                {
                    var exePath = Path.Combine(dir, exeName);
                    if (File.Exists(exePath))
                        yield return exePath;
                }
            }
        }
    }
}
