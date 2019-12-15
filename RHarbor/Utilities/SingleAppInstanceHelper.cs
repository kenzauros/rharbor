using System.Threading;

namespace kenzauros.RHarbor.Utilities
{
    /// <summary>
    /// Helps to run a single application instance.
    /// </summary>
    /// <see cref="https://dobon.net/vb/dotnet/process/checkprevinstance.html"/>
    internal static class SingleAppInstanceHelper
    {
        private static Mutex Mutex { get; set; }
        /// <summary>
        /// Tries to acquire mutex. Return false when other process exist.
        /// </summary>
        /// <returns></returns>
        public static bool TryStart()
        {
            var mutex = new Mutex(false, App.AssemblyName);
            try
            {
                // Request a mutex ownership
                var result = mutex.WaitOne(0, false);
                if (result)
                {
                    Mutex = mutex; // Hold mutex to release
                }
                else
                {
                    mutex.Close();
                }
                return result;
            }
            // as of .NET Framework 2.0
            catch (AbandonedMutexException)
            {
                // Another instance has exited without releasing mutex
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Relaeses the held mutex. Make sure to be called before application exit.
        /// </summary>
        public static void Exit()
        {
            Mutex?.ReleaseMutex();
            Mutex?.Close();
            Mutex = null;
        }
    }
}
