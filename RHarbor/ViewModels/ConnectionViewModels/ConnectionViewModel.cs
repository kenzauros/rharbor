using Reactive.Bindings;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace kenzauros.RHarbor.ViewModels
{
    internal class ConnectionViewModel : CompositeDisposableViewModelBase, IConnectionViewModel
    {
        protected MainWindowViewModel MainWindow => MainWindowViewModel.Singleton;

        public ReactiveProperty<bool> IsConnecting { get; set; } = new ReactiveProperty<bool>(mode: ReactivePropertyMode.DistinctUntilChanged);
        public ReactiveProperty<bool> IsConnected { get; set; } = new ReactiveProperty<bool>(mode: ReactivePropertyMode.DistinctUntilChanged);
        public ReactiveProperty<string> ConnectionName { get; set; } = new ReactiveProperty<string>();
        public ReactiveProperty<string> Host { get; set; } = new ReactiveProperty<string>();
        public ReactiveProperty<int?> Port { get; set; } = new ReactiveProperty<int?>();
        public ReactiveProperty<string> DisplayText { get; set; }

        public IConnectionViewModel Parent { get; set; }
        public ObservableCollection<IConnectionViewModel> Children { get; set; } = new ObservableCollection<IConnectionViewModel>();

        public AsyncReactiveCommand DisconnectCommand { get; set; }

        /// <summary>
        /// Whether this connection is root or not.
        /// </summary>
        public ReactiveProperty<bool> IsRoot { get; } = new ReactiveProperty<bool>(false);

        #region Log for user

        public ObservableCollection<string> Log { get; } = new ObservableCollection<string>();

        #endregion

        #region Connect/Disconnect

        public virtual Task Connect()
        {
            return Task.FromResult(false);
        }

        public virtual Task Disconnect()
        {
            return Task.FromResult(false);
        }

        #endregion
    }
}
