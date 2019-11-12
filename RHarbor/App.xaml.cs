using kenzauros.RHarbor.IPC;
using kenzauros.RHarbor.Models;
using kenzauros.RHarbor.Utilities;
using System;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Windows;

namespace kenzauros.RHarbor
{
    /// <summary>
    /// App.xaml Code bihind
    /// </summary>
    public partial class App : Application
    {
        public static string AssemblyName => System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;

        public App()
        {
            InitializeUnhandledExceptionHandlers();
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<AppDbContext, kenzauros.RHarbor.Migrations.Configuration>());
            MyLogger.Log("Application starting...");
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            ArgumentsHelper.SetArgs(e.Args);
            base.OnStartup(e);

            // Local method to enqueue connections specified with command-line argument
            bool EnqueueOpenRequestedConnections(IProcessCommander processCommander)
            {
                if (!ArgumentsHelper.HasConnectionSpecified) return false;
                foreach (var conn in ArgumentsHelper.SpecifiedConnections)
                {
                    MyLogger.Log($"Connection (Type: {conn.Type}, Id: {conn.ConnectionId}) has been enqueued.");
                    processCommander.Invoke(conn.Type, conn.ConnectionId);
                }
                return true;
            }

            if (SingleAppInstanceHelper.TryStart())
            {
                // Boot as an IPC host
                var service = new ProcessCommander
                {
                    ConnectionRequest = ConnectionRequest.Singleton,
                };
                var serviceHost = IPCService.OpenServiceHost(service);
                EnqueueOpenRequestedConnections(service);
            }
            else
            {
                // Boot as an IPC client
                var channel = IPCService.CreateServiceChannel();
                if (!EnqueueOpenRequestedConnections(channel))
                {
                    MyLogger.Log("Shutting down because another application instance has already run...");
                }
                channel.Activate();
                // Shutdown after activate the primary window
                Shutdown();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            SingleAppInstanceHelper.Exit();
        }

        /// <summary>
        /// Initialize unhandled exception handlers.
        /// </summary>
        protected void InitializeUnhandledExceptionHandlers()
        {
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        /// <summary>
        /// Processes an unhandled exception occurred in the UI thread.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            var exception = e.Exception as Exception;
            MyLogger.Log("Unhandled exception.", exception);
            if (ConfirmUnhandledException(exception, "UI Thread"))
            {
                e.Handled = true;
            }
            else
            {
                SingleAppInstanceHelper.Exit();
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// Processes an unhandled exception occurred in a background task.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            var exception = e.Exception.InnerException as Exception;
            MyLogger.Log("Unhandled exception.", exception);
            if (ConfirmUnhandledException(exception, "Background Task"))
            {
                e.SetObserved();
            }
            else
            {
                SingleAppInstanceHelper.Exit();
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// Processes eventually unhandled exceptions that allow to continue the program.
        /// </summary>
        /// <param name="e">Exception object</param>
        /// <param name="sourceName">Name of the exception source</param>
        /// <returns>true means user wanted to continue.</returns>
        protected bool ConfirmUnhandledException(Exception e, string sourceName)
        {
            var message = RHarbor.Properties.Resources.UnhandledException_Dialog_Confirmation_Message;
            if (e != null) message += $"\n({e.Message} @ {e.TargetSite.Name})";
            var result = MessageBox.Show(message, $"{RHarbor.Properties.Resources.UnhandledException_Dialog_Confirmation_Title} ({sourceName})", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            return result == MessageBoxResult.Yes;
        }

        /// <summary>
        /// Processes eventually unhandled exceptions.
        /// </summary>
        /// <param name="e">Exception object</param>
        /// <param name="sender"></param>
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var message = RHarbor.Properties.Resources.UnhandledException_Dialog_Notice_Message;
            if (e.ExceptionObject is Exception exception) message += $"\n({exception.Message} @ {exception.TargetSite.Name})";
            MessageBox.Show(message, RHarbor.Properties.Resources.UnhandledException_Dialog_Notice_Title, MessageBoxButton.OK, MessageBoxImage.Stop);
            Environment.Exit(1);
        }

    }
}
