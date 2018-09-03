using Reactive.Bindings.Extensions;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

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
            foreach (var item in Collection)
            {
                item.Dispose();
            }
            base.Dispose();
        }

        #endregion
    }
}
