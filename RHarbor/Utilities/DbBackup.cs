using kenzauros.RHarbor.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace kenzauros.RHarbor
{
    /// <summary>
    /// DB file backup utility
    /// </summary>
    internal class DbBackup
    {
        /// <summary>
        /// File extension of db files
        /// </summary>
        public const string DB_FILE_EXTENSION = ".db";

        /// <summary>
        /// Target directory path in which backup files are saved.
        /// </summary>
        public static string BackupDirectory =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), App.AssemblyName, "DbBackup");

        /// <summary>
        /// Get a hash of the latest backup file in <see cref="BackupDirectory"/>.
        /// Returns null if no directory or no files found.
        /// </summary>
        /// <returns></returns>
        private static async Task<string> GetLatestBackupFileHash()
        {
            if (!Directory.Exists(BackupDirectory)) return null;
            return await Task.Run(() =>
            {
                var fileInfo = new DirectoryInfo(BackupDirectory)
                    .EnumerateFiles("*" + DB_FILE_EXTENSION)
                    .OrderByDescending(_file => _file.CreationTime)
                    .FirstOrDefault();
                if (fileInfo != null)
                {
                    return MD5.GetFileMD5Hash(fileInfo.FullName);
                }
                return null;
            });
        }

        /// <summary>
        /// Backup the current DB file to <see cref="BackupDirectory"/> if modified.
        /// </summary>
        /// <returns></returns>
        public static async Task Execute()
        {
            var latestHash = await GetLatestBackupFileHash();
            if (latestHash != null)
            {
                var currentHash = MD5.GetFileMD5Hash(AppDbContext.DbFilePath);
                if (latestHash == currentHash) return;
            }
            await Task.Run(() =>
            {
                Directory.CreateDirectory(BackupDirectory);
                var newFilePath = Path.Combine(BackupDirectory,
                    Path.GetFileNameWithoutExtension(AppDbContext.DbFilePath) + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + DB_FILE_EXTENSION);
                File.Copy(AppDbContext.DbFilePath, newFilePath);
            });
        }

    }
}
