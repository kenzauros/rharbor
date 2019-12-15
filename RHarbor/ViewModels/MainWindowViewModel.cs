using kenzauros.RHarbor.Models;
using kenzauros.RHarbor.MvvmDialog;
using kenzauros.RHarbor.Utilities;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace kenzauros.RHarbor.ViewModels
{
    internal class MainWindowViewModel : CompositeDisposableViewModelBase, IDialogHost
    {
        #region Static

        public static MainWindowViewModel Singleton { get; private set; }

        #endregion

        public ReactiveProperty<bool> IsLoading { get; set; } = new ReactiveProperty<bool>();
        public SSHConnectionInfoManagementViewModel SSHConnectionInfos { get; set; } = new SSHConnectionInfoManagementViewModel();
        public RDPConnectionInfoManagementViewModel RDPConnectionInfos { get; set; } = new RDPConnectionInfoManagementViewModel();
        public ConnectionManagementViewModel Connections { get; set; } = new ConnectionManagementViewModel();
        public AppDbContext DbContext { get; private set; }

        public MainWindowViewModel()
        {
            if (Singleton != null)
            {
                throw new InvalidOperationException($"{nameof(MainWindowViewModel)} cannot be instantiated more than once.");
            }
            Singleton = this;
        }

        public async Task Load()
        {
            IsLoading.Value = true;
            try
            {
                await DbBackup.Execute();
            }
            catch (Exception ex)
            {
                MyLogger.Log("Exception occured on DB backup.", ex);
            }
            MyLogger.Log("Data loading...");
            DbContext = new AppDbContext();
            var cacheLoading = PortNumberCache.Load();
            var dbLoading = Task.Run(async () =>
            {
                await SSHConnectionInfo.RefreshAll(DbContext);
                DbContext.SSHConnectionInfos.ToList()
                    .ForEach(x => App.Current.Dispatcher.Invoke(() => SSHConnectionInfos.Items.Add(x)));
                DbContext.RDPConnectionInfos.ToList()
                    .ForEach(x => App.Current.Dispatcher.Invoke(() => RDPConnectionInfos.Items.Add(x)));
                DbContext.InitSecurePasswords();
            });
            await Task.WhenAll(new[]
            {
                cacheLoading,
                dbLoading,
            });
            MyLogger.Log("Data loaded.");
            IsLoading.Value = false;
            InitConnectionInvokeTimer();
        }

        #region Connection Invoking

        private DispatcherTimer ConnectionInvokeTimer { get; } = new DispatcherTimer();

        private void InitConnectionInvokeTimer()
        {
            ConnectionInvokeTimer.Interval = TimeSpan.FromMilliseconds(500);
            ConnectionInvokeTimer.Tick += async (s, e) =>
            {
                var list = new List<(ConnectionType Type, int Id)>();
                while (ConnectionRequest.Singleton.Queue.Count > 0)
                {
                    list.Add(ConnectionRequest.Singleton.Queue.Dequeue());
                }
                foreach (var (type, id) in list)
                {
                    try
                    {
                        switch (type)
                        {
                            case ConnectionType.SSH:
                                await SSHConnectionInfos.ConnectById(id);
                                break;
                            case ConnectionType.RDP:
                                await RDPConnectionInfos.ConnectById(id);
                                break;
                            default:
                                break;
                        }
                    }
                    catch (KeyNotFoundException ex)
                    {
                        MyLogger.Log($"ID {id} is not found in {type} connections.", ex);
                    }
                }
            };
            ConnectionInvokeTimer.Start();
        }

        #endregion

        #region IDialogHost

        public ReactiveProperty<IDialog> Dialog { get; } = new ReactiveProperty<IDialog>();

        #endregion

        #region IDisposable

        public override async void Dispose()
        {
            SSHConnectionInfos.Dispose();
            Connections.Dispose();
            DbContext?.Dispose();
            await PortNumberCache.Save();
            base.Dispose();
        }

        #endregion
    }
}
