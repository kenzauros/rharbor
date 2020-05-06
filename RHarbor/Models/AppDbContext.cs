using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;

namespace kenzauros.RHarbor.Models
{
    internal class AppDbContext : DbContext
    {
        public static readonly string DbFilePath;
        public static readonly DbContextOptions DbContextOptions;

        static AppDbContext()
        {
            DbFilePath = $@"{AppDomain.CurrentDomain.BaseDirectory}\RHarbor.db";
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlite(@$"Data Source={DbFilePath}");
            DbContextOptions = optionsBuilder.Options;
        }

        public AppDbContext() : base(DbContextOptions)
        {
        }

        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<RDPConnectionInfo> RDPConnectionInfos { get; set; }
        public DbSet<SSHConnectionInfo> SSHConnectionInfos { get; set; }

        public async Task<IEnumerable<ConnectionInfoBase>> EnumerateAllConnectionInfos()
        {
            var connections1 = await RDPConnectionInfos.ToListAsync();
            var connections2 = await SSHConnectionInfos.ToListAsync();
            return connections1
                .Cast<ConnectionInfoBase>()
                .Union(connections2);
        }

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
