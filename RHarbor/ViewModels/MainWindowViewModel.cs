using kenzauros.RHarbor.Models;
using kenzauros.RHarbor.MvvmDialog;
using kenzauros.RHarbor.Utilities;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
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
        public SettingsViewModel Settings { get; } = new();
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
                SSHConnectionInfo.All
                    .ForEach(x => App.Current.Dispatcher.Invoke(() => SSHConnectionInfos.Items.Add(x)));
                DbContext.RDPConnectionInfos.ToList()
                    .ForEach(x => App.Current.Dispatcher.Invoke(() => RDPConnectionInfos.Items.Add(x)));
                DbContext.DecryptPasswords();
                // ExternalProgramDefinitions
                var epdList = DbContext.ExternalProgramDefinitions.ToList();
                App.Current.Dispatcher.Invoke(() =>
                {
                    Settings.ExternalProgramDefinitionSettings.ResetItems(epdList);
                    SSHConnectionInfos.SetExternalProgramDefinitions(epdList);
                });
            });
            await Task.WhenAll(new[]
            {
                cacheLoading,
                dbLoading,
            });
            // Loading finished
            MyLogger.Log("Data loaded.");
            IsLoading.Value = false;
            InitConnectionInvokeTimer();
            InitConnectionListSizeControl();
        }

        #region Connection List Size control

        private GridLength PrevConnectionListSize = GridLength.Auto;
        public ReactiveProperty<GridLength> ConnectionListSize { get; } = new(new GridLength(0));
        public ReadOnlyReactiveProperty<bool> IsConnectionListVisible { get; private set; }
        public ReactiveCommand SwitchConnectionListCommand { get; } = new();

        private void InitConnectionListSizeControl()
        {
            IsConnectionListVisible = ConnectionListSize.Select(x => x.Value != 0).ToReadOnlyReactiveProperty();
            SwitchConnectionListCommand.Subscribe(() =>
            {
                if (ConnectionListSize.Value.Value > 0)
                {
                    ShrinkConnectionListSize();
                }
                else
                {
                    ExpandConnectionListSize();
                }
            }).AddTo(Disposable);
            Connections.Collection.CollectionChanged += Connections_CollectionChanged;
        }

        private void ShrinkConnectionListSize()
        {
            PrevConnectionListSize = ConnectionListSize.Value;
            ConnectionListSize.Value = new GridLength(0);
        }

        private void ExpandConnectionListSize()
        {
            if (IsConnectionListVisible.Value) return;
            if (!Connections.Collection.Any() && PrevConnectionListSize.IsAuto)
            {
                ConnectionListSize.Value = new GridLength(250);
            }
            else
            {
                ConnectionListSize.Value = PrevConnectionListSize;
            }
        }

        private void Connections_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (sender is ICollection<IConnectionViewModel> col)
            {
                if (col.Count == 0)
                {
                    ShrinkConnectionListSize();
                }
                else
                {
                    ExpandConnectionListSize();
                }
            }
        }

        #endregion

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

        #region Existing Group List

        /// <summary>
        /// Lists already existing group names to bind to the combo box in the property editor.
        /// </summary>
        public List<ConnectionGroup> ExistingGroupList =>
            RDPConnectionInfos.Groups.Concat(SSHConnectionInfos.Groups)
            .OrderBy(x => x.DisplayName)
            .Where(x => !string.IsNullOrEmpty(x.Name) && x.Name != ConnectionInfoManagementViewModel<SSHConnectionInfo>.AllGroupName)
            .ToList();

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
