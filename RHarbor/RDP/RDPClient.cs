using kenzauros.RHarbor.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace kenzauros.RHarbor
{
    internal class RDPClient
    {
        /// <summary>
        /// Information for the connection.
        /// </summary>
        public RDPConnectionInfo ConnectionInfo { get; set; }

        /// <summary>
        /// Internal process.
        /// </summary>
        public Process Process { get; private set; }

        /// <summary>
        /// Whether the RDP process has started.
        /// </summary>
        public bool IsOpened { get; private set; }

        /// <summary>
        /// RDP session closed.
        /// </summary>
        public event EventHandler Closed;

        public RDPClient(RDPConnectionInfo info)
        {
            ConnectionInfo = info;
        }

        /// <summary>
        /// Starts a RDP process.
        /// </summary>
        /// <returns></returns>
        public Task Open(string actualHost = null, int? actualPort = null)
        {
            if (Process != null)
            {
                throw new InvalidOperationException("Process has been existed.");
            }
            // Save as a .rdp file
            var filename = string.IsNullOrWhiteSpace(ConnectionInfo.Name)
                ? $"{ConnectionInfo.Host}_{ConnectionInfo.Port}"
                : ConnectionInfo.Name;
            filename = PathUtil.CleanFilename(filename) + ".rdp";
            var filepath = PathUtil.GetAppTempFilePath(filename, "rdp");
            ConnectionInfo.SaveAs(filepath, actualHost, actualPort);
            // Organize arguments for mstsc.exe
            var port = actualPort ?? (ConnectionInfo.Port > 0 ? ConnectionInfo.Port : 3389);
            var args = new List<string>
            {
                $"\"{filepath}\"",
                $"/v:{actualHost ?? ConnectionInfo.Host}:{port}",
            };
            if (ConnectionInfo.Admin) args.Add("/admin");
            var app = new ProcessStartInfo
            {
                UseShellExecute = false,
                FileName = @"C:\Windows\System32\mstsc.exe",
                Arguments = string.Join(" ", args),
            };
            // Start a process
            Process = Process.Start(app);
            Process.EnableRaisingEvents = true;
            Process.Exited += (s, e) =>
            {
                IsOpened = false;
                Closed?.Invoke(this, e);
            };
            IsOpened = true;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Closes the RDP process.
        /// </summary>
        /// <returns></returns>
        public Task Close()
        {
            if (Process?.HasExited == false)
            {
                Process?.Kill();
                Process?.Close();
            }
            Process = null;
            IsOpened = false;
            return Task.CompletedTask;
        }

    }
}
