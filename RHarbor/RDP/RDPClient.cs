using kenzauros.RHarbor.Models;
using kenzauros.RHarbor.RDP;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        /// Attempts to use FreeRDP when a compatible binary is found next to the application
        /// executable. Falls back to <c>mstsc.exe</c> when FreeRDP is unavailable or fails
        /// to start.
        /// </summary>
        /// <returns></returns>
        public Task Open(string actualHost = null, int? actualPort = null)
        {
            if (Process != null)
            {
                throw new InvalidOperationException("Process has been existed.");
            }

            // Try FreeRDP candidates in priority order; fall back to mstsc on failure.
            var freerdpCandidates = FreeRDPLocator.FindExecutables().ToList();
            foreach (var exePath in freerdpCandidates)
            {
                if (TryOpenWithFreeRDP(exePath, actualHost, actualPort))
                    return Task.CompletedTask;
            }

            // Fall back to mstsc.exe
            OpenWithMstsc(actualHost, actualPort);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Attempts to start a FreeRDP process using the specified executable.
        /// Connection parameters are passed via stdin using the <c>/args-from:stdin</c> option.
        /// </summary>
        /// <param name="exePath">Full path to the FreeRDP executable.</param>
        /// <param name="actualHost">Override host, or <c>null</c> to use <see cref="ConnectionInfo"/>.</param>
        /// <param name="actualPort">Override port, or <c>null</c> to use <see cref="ConnectionInfo"/>.</param>
        /// <returns><c>true</c> if the process started successfully; <c>false</c> otherwise.</returns>
        private bool TryOpenWithFreeRDP(string exePath, string actualHost, int? actualPort)
        {
            try
            {
                var host = actualHost ?? ConnectionInfo.Host;
                var port = actualPort ?? (ConnectionInfo.Port > 0 ? ConnectionInfo.Port : 3389);

                var argsLines = new List<string>
                {
                    $"/v:{host}:{port}",
                };

                if (!string.IsNullOrEmpty(ConnectionInfo.Username))
                    argsLines.Add($"/u:{ConnectionInfo.Username}");

                if (!string.IsNullOrEmpty(ConnectionInfo.RawPassword))
                    argsLines.Add($"/p:{ConnectionInfo.RawPassword}");

                if (ConnectionInfo.DesktopWidth.HasValue && ConnectionInfo.DesktopHeight.HasValue)
                    argsLines.Add($"/size:{ConnectionInfo.DesktopWidth.Value}x{ConnectionInfo.DesktopHeight.Value}");

                argsLines.Add("+clipboard");
                argsLines.Add("/cert:ignore");

                if (ConnectionInfo.Admin)
                    argsLines.Add("+admin");

                var stdinArgs = string.Join("\n", argsLines) + "\n";

                var psi = new ProcessStartInfo
                {
                    FileName = exePath,
                    Arguments = "/args-from:stdin",
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = false,
                    RedirectStandardError = false,
                    CreateNoWindow = false,
                };

                var proc = Process.Start(psi);
                if (proc == null)
                    return false;

                proc.StandardInput.Write(stdinArgs);
                proc.StandardInput.Close();

                Process = proc;
                Process.EnableRaisingEvents = true;
                Process.Exited += (s, e) =>
                {
                    IsOpened = false;
                    Closed?.Invoke(this, e);
                };
                IsOpened = true;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Starts <c>mstsc.exe</c> with a temporary .rdp file.
        /// </summary>
        private void OpenWithMstsc(string actualHost, int? actualPort)
        {
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
