using kenzauros.RHarbor.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kenzauros.RHarbor
{
    internal class DbBackup
    {
        public const string DB_FILE_EXTENSION = ".db";

        public static string BackupDirectory =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), App.AssemblyName, "DbBackup");

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
