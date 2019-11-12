using kenzauros.RHarbor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace kenzauros.RHarbor.Utilities
{
    internal static class ArgumentsHelper
    {
        /// <summary>
        /// Comand-line arguments passed through App.Startup
        /// </summary>
        public static string[] Args { get; private set; }

        /// <summary>
        /// Whether available connection informations specified or not.
        /// </summary>
        public static bool HasConnectionSpecified => SpecifiedConnections?.Any() == true;

        /// <summary>
        /// Connection informations extracted from "open" argument.
        /// </summary>
        public static IEnumerable<(ConnectionType Type, int ConnectionId)> SpecifiedConnections { get; private set; }

        /// <summary>
        /// Sets command-line arguments to extract informations.
        /// </summary>
        /// <param name="args"></param>
        public static void SetArgs(params string[] args)
        {
            Args = args;
            foreach (var arg in args)
            {
                // Argument to specify connections the user want to open
                var openArgMatch = Regex.Match(arg, @"^(/|--)open=(.+)?");
                if (openArgMatch.Success)
                {
                    ExtractOpenArg(openArgMatch.Groups[2].Value);
                }
            }
        }

        static void ExtractOpenArg(string argValue)
        {
            var matches = Regex.Matches(argValue, @"(rdp|ssh):(\d+)", RegexOptions.IgnoreCase);
            SpecifiedConnections = matches
                .Cast<Match>()
                .Where(x => x.Success)
                .Select(x =>
                {
                    var type = (x.Groups[1].Value == "rdp") ? ConnectionType.RDP : ConnectionType.SSH;
                    int.TryParse(x.Groups[2].Value, out var id);
                    return (type, id);
                })
                .ToList();
        }
    }
}
