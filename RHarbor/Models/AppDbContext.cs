using System;
using System.Data.Common;
using System.Data.Entity;
using System.Data.SQLite;
using System.IO;
using System.Security;
using System.Threading.Tasks;

namespace kenzauros.RHarbor.Models
{
    internal class AppDbContext : DbContext
    {
        public static readonly string DbFilePath;

        static AppDbContext()
        {
            DbFilePath = $@"{AppDomain.CurrentDomain.BaseDirectory}\RHarbor.db";
        }

        public AppDbContext() : base(new SQLiteConnection($"DATA Source={DbFilePath}"), false)
        {
        }

        public AppDbContext(DbConnection connection) : base(connection, true)
        {
        }

        public DbSet<RDPConnectionInfo> RDPConnectionInfos { get; set; }
        public DbSet<SSHConnectionInfo> SSHConnectionInfos { get; set; }

        public void InitSecurePasswords()
        {
            foreach (var item in SSHConnectionInfos)
                item.InitSecurePassword();
            foreach (var item in RDPConnectionInfos)
                item.InitSecurePassword();
        }

        public async Task UpdateSSHFingerPrint(SSHConnectionInfo info, string fingerprint)
        {
            var record = await SSHConnectionInfos.FirstOrDefaultAsync(x => x.Id == info.Id);
            if (record != null)
            {
                record.ExpectedFingerPrint = fingerprint;
                await SaveChangesAsync();
            }
        }

        public async Task SavePassword(SSHConnectionInfo info, bool savePassword, SecureString password)
        {
            var record = await SSHConnectionInfos.FirstOrDefaultAsync(x => x.Id == info.Id);
            if (record != null)
            {
                record.SavePassword = savePassword;
                record.SecurePassword = savePassword ? password : null;
                await SaveChangesAsync();
            }
        }
    }
}
