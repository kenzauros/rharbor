using Reactive.Bindings.Extensions;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace kenzauros.RHarbor.ViewModels
{
    internal class ConnectionManagementViewModel : CompositeDisposableViewModelBase
    {
        public ObservableCollection<IConnectionViewModel> Collection { get; } = new ObservableCollection<IConnectionViewModel>();

        public ConnectionManagementViewModel()
        {
            Collection.CollectionChanged += CollectionChanged;
        }

        private void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (IConnectionViewModel item in e.NewItems)
                {
                    item.IsConnected.Subscribe(isConnected =>
                    {
                        if (!isConnected)
                        {
                            Collection.Remove(item);
                        }
                    }).AddTo(Disposable);
                }
            }
        }

        #region IDisposable

        public override void Dispose()
        {
            IConnectionViewModel[] connections = Collection.ToArray();
            foreach (IConnectionViewModel item in connections)
            {
                item?.Dispose();
            }
            base.Dispose();
        }

        #endregion
    }
}
