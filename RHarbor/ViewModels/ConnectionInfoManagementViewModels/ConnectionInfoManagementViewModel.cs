using kenzauros.RHarbor.Models;
using kenzauros.RHarbor.MvvmDialog;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Data;

namespace kenzauros.RHarbor.ViewModels
{
    internal class ConnectionInfoManagementViewModel<T> : CompositeDisposableViewModelBase
        where T : class, IConnectionInfo, IRewriteable, new()
    {
        protected MainWindowViewModel MainWindow => MainWindowViewModel.Singleton;

        public ObservableCollection<T> Items { get; set; } = new ObservableCollection<T>();
        public ReactiveProperty<string> FilterText { get; set; } = new ReactiveProperty<string>();

        public AsyncReactiveCommand<T> RemoveItemCommand { get; set; } = new AsyncReactiveCommand<T>();
        public AsyncReactiveCommand<T> ConnectCommand { get; set; } = new AsyncReactiveCommand<T>();

        public ReactiveCommand StartEditCommand { get; set; }
        public ReactiveProperty<T> SelectedItem { get; set; } = new ReactiveProperty<T>();
        public ReactiveProperty<bool> IsItemEditing { get; set; } = new ReactiveProperty<bool>(mode: ReactivePropertyMode.RaiseLatestValueOnSubscribe);
        public ReadOnlyReactiveProperty<bool> IsNotItemEditing { get; set; }
        public ReactiveProperty<T> EditingItem { get; set; } = new ReactiveProperty<T>();
        public ReactiveCommand AddNewItemCommand { get; set; }
        public ReactiveCommand SaveChangesCommand { get; set; }
        public ReactiveCommand DiscardChangesCommand { get; set; }
        public ReadOnlyReactiveProperty<bool> IsItemSelected { get; set; }

        public ConnectionInfoManagementViewModel()
        {
            AddNewItemCommand = IsItemEditing.Inverse().ToReactiveCommand();
            AddNewItemCommand.Subscribe(() =>
            {
                SelectedItem.Value = null;
                IsItemEditing.Value = true;
            }).AddTo(Disposable);

            RemoveItemCommand.Subscribe(async item =>
            {
                if (await Remove(item))
                {
                    Items.Remove(item);
                }
            }).AddTo(Disposable);

            ConnectCommand.Subscribe(item => Connect(item)).AddTo(Disposable);

            IsItemSelected = SelectedItem.Select(x => x != null).ToReadOnlyReactiveProperty();

            IsItemEditing.Subscribe(isItemEditing =>
            {
                EditingItem.Value = isItemEditing
                    ? ((SelectedItem.Value == null) ? new T() : SelectedItem.Value.CloneDeep())
                    : SelectedItem.Value;
            }).AddTo(Disposable);
            IsNotItemEditing = IsItemEditing.Inverse().ToReadOnlyReactiveProperty();

            SelectedItem.Subscribe(x =>
            {
                IsItemEditing.Value = false;
            }).AddTo(Disposable);

            StartEditCommand = IsItemSelected
                .CombineLatest(IsItemEditing.Inverse(), (a, b) => a && b)
                .ToReactiveCommand();
            StartEditCommand.Subscribe(() =>
            {
                IsItemEditing.Value = true;
            }).AddTo(Disposable);

            DiscardChangesCommand = IsItemEditing.ToReactiveCommand();
            DiscardChangesCommand.Subscribe(() =>
            {
                IsItemEditing.Value = false;
            }).AddTo(Disposable);

            SaveChangesCommand = IsItemEditing.ToReactiveCommand();
            SaveChangesCommand.Subscribe(async () =>
            {
                var selectedItem = SelectedItem.Value;
                var item = EditingItem.Value;
                try
                {
                    var result = await Save(item);
                    if (result) // ADDED
                    {
                        Items.Add(item);
                    }
                    else // UPDATED
                    {
                        var oldItem = Items.FirstOrDefault(x => x.Id == item.Id);
                        if (oldItem != null)
                        {
                            var index = Items.IndexOf(oldItem);
                            if (index >= 0)
                            {
                                Items.RemoveAt(index);
                                Items.Insert(index, item);
                            }
                        }
                    }
                }
                catch (OperationCanceledException) // User manually canceled
                {
                    return;
                }
                SelectedItem.Value = item;
                IsItemEditing.Value = false;
            }).AddTo(Disposable);

            FilterText.Subscribe(filter =>
            {
                var collectionView = CollectionViewSource.GetDefaultView(Items);
                if (string.IsNullOrEmpty(filter))
                {
                    collectionView.Filter = null;
                }
                else
                {
                    var regex = new Regex(filter, RegexOptions.IgnoreCase);
                    collectionView.Filter = x => regex.IsMatch(((T)x).Name) || regex.IsMatch(((T)x).Host);
                }
            }).AddTo(Disposable);
        }

        protected virtual async Task<bool> Remove(T item)
        {
            var result = await MainWindow.ShowConfirmationDialog($"Are you sure you want to remove \"{item.ToString()}\"?\nThis operation cannot be canceled.", "Removing connection info");
            if (!result) return false;
            try
            {
                MyLogger.Log($"Removing {item.ToString()}...");
                var currentItem = MainWindow.DbContext.Set<T>().FirstOrDefault(x => x.Id == item.Id);
                if (currentItem != null)
                {
                    MainWindow.DbContext.Set<T>().Remove(currentItem);
                    await MainWindow.DbContext.SaveChangesAsync();
                }
                return true;
            }
            catch (Exception ex)
            {
                MyLogger.Log($"Failed to remove {item.ToString()}.", ex);
                await MainWindow.ShowMessageDialog(ex.Message, "Removing connection info");
                return false;
            }
        }

        protected virtual async Task<bool> Save(T item)
        {
            var result = await MainWindow.ShowConfirmationDialog("Save the changes?", "Saving connection info");
            if (!result) throw new OperationCanceledException();
            try
            {
                MyLogger.Log($"Saving {item.ToString()}...");
                var res = false;
                var currentItem = MainWindow.DbContext.Set<T>().FirstOrDefault(x => x.Id == item.Id);
                if (currentItem == null)
                {
                    currentItem = new T();
                    MainWindow.DbContext.Set<T>().Add(currentItem);
                    res = true; // returning true means ADDED
                }
                currentItem.RewriteWith(item);
                await MainWindow.DbContext.SaveChangesAsync();
                return res;
            }
            catch (Exception ex)
            {
                MyLogger.Log($"Failed to save {item.ToString()}.", ex);
                await MainWindow.ShowMessageDialog(ex.Message, "Saving connection info");
                return false;
            }
        }

        protected virtual async Task Connect(T item)
        {
            var result = await MainWindow.ShowConfirmationDialog($"Start to connect to \"{item.Name}\"?", "Starting connection");
            if (!result) return;
            MyLogger.Log($"Connecting to {item.ToString()}...");
            var conn = ConnectionViewModel<T>.CreateFromConnectionInfo(item);
            MainWindow.Connections.Collection.Add(conn);
            try
            {
                await conn.Connect();
            }
            catch (Exception ex)
            {
                MainWindow.Connections.Collection.Remove(conn);
                switch (ex)
                {
                    case OperationCanceledException oce: // User canceled manually
                        MyLogger.Log($"Canceled to connect to {item.ToString()}.");
                        return;
                }
                MyLogger.Log($"Failed to connect to {item.ToString()}.", ex);
                await MainWindow.ShowMessageDialog($"Failed to connect to \"{item.Name}\".\n{ex.Message}", "Starting connection");
            }
        }
    }
}
