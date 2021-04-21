using kenzauros.RHarbor.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace kenzauros.RHarbor.Models
{
    [Serializable]
    [Table("external_program_definitions")]
    internal class ExternalProgramDefinition : RewriteableBase
    {
        [Key]
        [Browsable(false)]
        public long Id { get => _Id; set => SetProp(ref _Id, value); }
        private long _Id;

        [Required]
        [PropertyOrder(1)]
        [LocalizedDisplayName(nameof(ExternalProgramDefinition) + "_" + nameof(Name))]
        [LocalizedDescription(nameof(ExternalProgramDefinition) + "_" + nameof(Name) + "_Description")]
        public string Name { get => _Name ?? ""; set => SetProp(ref _Name, value ?? ""); }
        private string _Name;

        [PropertyOrder(2)]
        [LocalizedDisplayName(nameof(ExternalProgramDefinition) + "_" + nameof(ExePath))]
        [LocalizedDescription(nameof(ExternalProgramDefinition) + "_" + nameof(ExePath) + "_Description")]
        public string ExePath { get => _ExePath; set => SetProp(ref _ExePath, value); }
        private string _ExePath;

        [PropertyOrder(3)]
        [LocalizedDisplayName(nameof(ExternalProgramDefinition) + "_" + nameof(Arguments))]
        [LocalizedDescription(nameof(ExternalProgramDefinition) + "_" + nameof(Arguments) + "_Description")]
        public string Arguments { get => _Arguments; set => SetProp(ref _Arguments, value); }
        private string _Arguments;

        [Required]
        [PropertyOrder(4)]
        [LocalizedDisplayName(nameof(ExternalProgramDefinition) + "_" + nameof(CopyToClipboard))]
        [LocalizedDescription(nameof(ExternalProgramDefinition) + "_" + nameof(CopyToClipboard) + "_Description")]
        public bool CopyToClipboard { get => _CopyToClipboard; set => SetProp(ref _CopyToClipboard, value); }
        private bool _CopyToClipboard;

        public override string ToString() => Name?.ToString();

        #region Invocation

        /// <summary>
        /// Invokes the program with the SSH connection information.
        /// </summary>
        /// <param name="info"></param>
        public void Invoke(SSHConnectionInfo info)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));
            if (!CopyToClipboard && !File.Exists(ExePath))
                throw new FileNotFoundException(Resources.ExternalProgramDefinition_Exception_ExeNotFound);

            var placeholders = new Dictionary<string, Func<SSHConnectionInfo, string>>
            {
                { "{host}", info => info.Host },
                { "{port}", info => info.Port.ToString() },
                { "{username}", info => info.Username },
                { "{password}", info => info.RawPassword },
                { "{keyfile}", info => info.PrivateKeyFilePath },
            };
            var arguments = placeholders.Aggregate(Arguments,
                (prev, ph) => prev.Replace(ph.Key, ph.Value.Invoke(info)));
            var exePath = ExePath.Contains(" ") ? $"\"{ExePath}\"" : ExePath;
            if (CopyToClipboard)
            {
                try
                {
                    Clipboard.SetText($"{exePath} {arguments}");
                    MyLogger.Log($"Set to clipboard; {Name} (Connection: {info.Name})");
                }
                catch (Exception ex)
                {
                    MyLogger.Log($"Failed to set to clipboard; {Name} (Connection: {info.Name})", ex);
                    throw;
                }
            }
            else
            {
                MyLogger.Log($"Invoking...; {Name} (Connection: {info.Name})");
                try
                {
                    ProcessStartInfo psi = new()
                    {
                        FileName = exePath,
                        Arguments = arguments,
                        UseShellExecute = true,
                    };
                    Process.Start(psi);
                    MyLogger.Log($"Invoked; {Name} (Connection: {info.Name})");
                }
                catch (Exception ex)
                {
                    MyLogger.Log($"Failed to invoke; {Name} (Connection: {info.Name})", ex);
                    throw;
                }
            }
        }

        #endregion

        #region Factory for Usual Programs

        public static ExternalProgramDefinition CreateOpenSSHDefinition()
        {
            // https://docs.microsoft.com/ja-jp/windows-server/administration/openssh/openssh_overview
            var definition = new ExternalProgramDefinition
            {
                Name = "OpenSSH (Clipboard)",
                ExePath = "ssh",
                Arguments = "-i \"{keyfile}\" {username}@{host}:{port}",
                CopyToClipboard = true,
            };
            return definition;
        }

        public static ExternalProgramDefinition CreateTeraTermDefinition()
        {
            // https://ttssh2.osdn.jp/manual/4/ja/commandline/ttssh.html
            var exePath = FindExe("teraterm", "ttermpro.exe");
            var definition = new ExternalProgramDefinition
            {
                Name = "Tera Term",
                ExePath = exePath,
                Arguments = "{host}:{port} /ssh2 /user=\"{username}\" /passwd=\"{password}\" /keyfile=\"{keyfile}\" /auth=publickey",
                CopyToClipboard = false,
            };
            return definition;
        }

        public static ExternalProgramDefinition CreatePuttyDefinition()
        {
            // https://documentation.help/PuTTY/using-cmdline-session.html
            var exePath = FindExe("PuTTY", "putty.exe");
            var definition = new ExternalProgramDefinition
            {
                Name = "PuTTY",
                ExePath = exePath,
                Arguments = "-ssh -P {port} -l {username} -pw {password} -i \"{keyfile}.ppk\" {host}",
                CopyToClipboard = false,
            };
            return definition;
        }

        public static ExternalProgramDefinition CreateRLoginDefinition()
        {
            // http://nanno.dip.jp/softlib/man/rlogin/#CMDLINE
            var definition = new ExternalProgramDefinition
            {
                Name = "RLogin",
                ExePath = "RLogin.exe",
                Arguments = "/ssh /inusea /ip {host} /port {port} /user \"{username}\" /pass \"{password}\" /idkey \"{keyfile}\"",
                CopyToClipboard = false,
            };
            return definition;
        }

        public static string FindExe(string expectedFolderName, string exeFileName)
        {
            var dirCandidates = new[]
            {
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles),
                Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFilesX86),
            };
            return dirCandidates
                .Select(x => Path.Combine(x, expectedFolderName, exeFileName))
                .FirstOrDefault(p => File.Exists(p))
                ?? null;
        }

        #endregion
    }
}
