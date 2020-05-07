using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using kenzauros.RHarbor.Rpc;
using kenzauros.RHarbor.Utilities;
using System;
using System.Linq;
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
            MyLogger.Log("Application starting...");
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            ArgumentsHelper.SetArgs(e.Args);

            base.OnStartup(e);

            var isMainProcess = SingleAppInstanceHelper.TryStart();
            if (isMainProcess)
            {
                try
                {
                    var service = new CommanderService
                    {
                        ActivateFunc = async () => await Dispatcher.InvokeAsync(() => App.Current?.MainWindow?.Activate()),
                    };
                    var server = new Server
                    {
                        Services = { Commander.BindService(service) },
                        Ports = { new ServerPort("localhost", CommanderService.DefaultPort, ServerCredentials.Insecure) },
                    };
                    server.Start();
                    MyLogger.Log($"gRPC Server: Started.");
                }
                catch (Exception ex)
                {
                    MyLogger.Log($"gRPC Server: Failed to start.", ex);
                }
            }

            static Commander.CommanderClient GetCommanderClient()
            {
                var channel = new Channel($"localhost:{CommanderService.DefaultPort}", ChannelCredentials.Insecure);
                return new Commander.CommanderClient(channel);
            }

            var specifiedConnections = ArgumentsHelper.SpecifiedConnections?.ToList();
            if (specifiedConnections?.Count > 0)
            {
                var args = specifiedConnections.Select(x => $"{x.Type}:{x.ConnectionId}");
                MyLogger.Log($"Trying to start connections specified with arguments... {string.Join(", ", args)}");
                try
                {
                    var client = GetCommanderClient();

                    foreach (var conn in ArgumentsHelper.SpecifiedConnections)
                    {
                        var reply = client.StartConnect(new StartConnectRequest
                        {
                            ConnectionType = conn.Type.ToString(),
                            ConnectionId = conn.ConnectionId,
                        });
                        MyLogger.Log($"Connection request (Type: {conn.Type}, Id: {conn.ConnectionId}): {(reply.Success ? "Enqueued" : "Failed")} {reply.Message}");
                    }
                }
                catch (Exception ex)
                {
                    MyLogger.Log($"Failed starting {(isMainProcess ? "as the main process" : "argument command")}.", ex);
                }
            }

            if (!isMainProcess)
            {
                try
                {
                    var client = GetCommanderClient();
                    // Shutdown after activate the primary window
                    var reply = client.Activate(new Empty());
                    MyLogger.Log($"Window Activation: {(reply.Success ? "Success" : "Failed")} {reply.Message}");
                }
                catch (Exception ex)
                {
                    MyLogger.Log($"Failed to activate the main window.", ex);
                }
                finally
                {
                    Shutdown();
                }
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
