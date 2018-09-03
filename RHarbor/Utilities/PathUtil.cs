using System;
using System.IO;
using System.Text.RegularExpressions;

namespace kenzauros.RHarbor
{
    internal static class PathUtil
    {
        public static string GetAppTempDirectory(string subDirectory = null)
        {
            var tempDir = Path.GetTempPath();
            if (subDirectory != null)
            {
                tempDir = Path.Combine(tempDir, subDirectory);
            }
            Directory.CreateDirectory(tempDir);
            return tempDir;
        }

        public static string GetAppTempFilePath(string filename, string subDirectory = null)
        {
            var tempDir = GetAppTempDirectory(subDirectory);
            return Path.Combine(tempDir, filename);
        }

        private static Regex InvalidCharRegex;

        public static string CleanFilename(string filename, string alt = null)
        {
            if (InvalidCharRegex == null)
            {
                var chars = Path.GetInvalidFileNameChars();
                var text = new string(chars);
                InvalidCharRegex = new Regex($@"[{text}]");
            }
            if (alt != null && InvalidCharRegex.IsMatch(alt))
            {
                throw new ArgumentException($"Alternate text includes invalid file name chars.", nameof(alt));
            }
            return InvalidCharRegex.Replace(filename, alt ?? "");
        }
    }
}
