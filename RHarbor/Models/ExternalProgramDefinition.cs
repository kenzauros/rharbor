using kenzauros.RHarbor.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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

        [Browsable(false)]
        public int? SortOrder { get => _SortOrder; set => SetProp(ref _SortOrder, value); }
        private int? _SortOrder;

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

            string arguments = CreateArguments(info);
            string exePath = ExePath.Contains(" ") ? $"\"{ExePath}\"" : ExePath;
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

        /// <summary>
        /// Creates a command-line arguments from the template (<see cref="Arguments"/> property) with the connection information.
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public string CreateArguments(SSHConnectionInfo info)
        {
            string arguments = Arguments;
            Dictionary<string, string> paramDict = new()
            {
                { "host", info.Host },
                { "port", info.Port.ToString() },
                { "username", info.Username },
                { "password", info.RawPassword },
                { "keyfile", info.PrivateKeyFilePath },
            };
            if (info.ConnectionParameters?.Count > 0)
            {
                foreach (SSHConnectionParameter kv in info.ConnectionParameters)
                {
                    if (!paramDict.ContainsKey(kv.Key))
                    {
                        paramDict.Add(kv.Key, kv.Value);
                    }
                }
            }
            return ReplaceConnectionParameters(arguments, paramDict);
        }

        static Regex NestedBracketsRegex = new(@"{[^{}]*({[^{}]*}[^{}]*)*[^{}]*}");
        static Regex ConditionalOperator = new(@"^{(.+?)\?(.+?):(.+?)}$");
        static Regex ParameterPlaceholderRegex = new(@"(?<!{)\{([^{=:?]+?)(:([^{=:?]+?))?(?<!})\}");

        /// <summary>
        /// Replaces placeholders in the arguments with the connection parameters.
        /// </summary>
        /// <param name="arguments"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static string ReplaceConnectionParameters(string arguments, Dictionary<string, string> parameters)
        {
            string getParamValue(string key, string fallbackValue)
                => parameters.TryGetValue(key, out string value)
                    ? value // parameter value
                    : fallbackValue; // fallback value

            // Nested brackets ({})
            arguments = NestedBracketsRegex.Replace(arguments, m1 =>
                // Conditional operator (condition ? positive : negative)
                ConditionalOperator.Replace(m1.Value, m =>
                    {
                        string condition = m.Groups[1].Value;
                        string positive = m.Groups[2].Value;
                        string negative = m.Groups[3].Value;
                        Match conditionExp = Regex.Match(condition, @"^(.+?)(=|!=)(.+?)$");
                        if (conditionExp.Success)
                        {
                            // = or != operator with 2 operands
                            string value = getParamValue(conditionExp.Groups[1].Value, "");
                            bool isMatch = conditionExp.Groups[2].Value switch
                            {
                                "=" => value == conditionExp.Groups[3].Value,
                                _ => value != conditionExp.Groups[3].Value,
                            };
                            return isMatch ? positive : negative;
                        }
                        else
                        {
                            // single operand (with/wo ! operator)
                            Match match = Regex.Match(condition, @"^(!?)(.+)$");
                            if (match.Success)
                            {
                                bool not = match.Groups[1].Value == "!"; // ! exists at first
                                string value = getParamValue(match.Groups[2].Value, "");
                                return not ^ !string.IsNullOrEmpty(value)
                                    ? positive
                                    : negative;
                            }
                            else
                            {
                                return m.Value;
                            }
                        }
                    }));
            arguments = ParameterPlaceholderRegex.Replace(arguments, m =>
                {
                    string key = m.Groups[1].Value;
                    string fallbackValue = m.Groups.Count == 4 ? m.Groups[3].Value : string.Empty;
                    return getParamValue(key, fallbackValue);
                })
                .Replace(@"{{", "{")
                .Replace(@"}}", "}");
            return arguments;
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
                Arguments = "{host}:{port} /ssh2 /user=\"{username}\" /passwd=\"{password}\" /keyfile=\"{keyfile}\" /auth={keyfile?publickey:password}",
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
