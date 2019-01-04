using kenzauros.RHarbor.Models;
using kenzauros.RHarbor.MvvmDialog;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

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
        }

        #region IDialogHost

        public ReactiveProperty<IDialog> Dialog { get; } = new ReactiveProperty<IDialog>();

        #endregion

        #region IDisposable

        public override async void Dispose()
        {
            SSHConnectionInfos.Dispose();
            Connections.Dispose();
            DbContext.Dispose();
            await PortNumberCache.Save();
            base.Dispose();
        }

        #endregion
    }
}
