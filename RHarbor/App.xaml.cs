using System;
using System.Threading.Tasks;
using System.Windows;

namespace kenzauros.RHarbor
{
    /// <summary>
    /// App.xaml Code bihind
    /// </summary>
    public partial class App : Application
    {

        public App()
        {
            InitializeUnhandledExceptionHandlers();
            MyLogger.Log("Application starting...");
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
            var message = $"Unhandled exception occured.\nAre you want to continute the program?";
            if (e != null) message += $"\n({e.Message} @ {e.TargetSite.Name})";
            var result = MessageBox.Show(message, $"Unhandled exception ({sourceName})", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            return result == MessageBoxResult.Yes;
        }

        /// <summary>
        /// Processes eventually unhandled exceptions.
        /// </summary>
        /// <param name="e">Exception object</param>
        /// <param name="sender"></param>
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var message = $"Unhandled exception occured.";
            if (e.ExceptionObject is Exception exception) message += $"\n({exception.Message} @ {exception.TargetSite.Name})";
            MessageBox.Show(message, "Unhandled exception", MessageBoxButton.OK, MessageBoxImage.Stop);
            Environment.Exit(1);
        }

    }
}
