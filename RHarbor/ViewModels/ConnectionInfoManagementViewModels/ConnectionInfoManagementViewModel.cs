using kenzauros.RHarbor.Models;
using kenzauros.RHarbor.MvvmDialog;
using kenzauros.RHarbor.Properties;
using kenzauros.RHarbor.Utilities;
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

        public ReactiveCommand<T> RemoveItemCommand { get; set; } = new ReactiveCommand<T>();
        public ReactiveCommand<T> ConnectCommand { get; set; } = new ReactiveCommand<T>();

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
                    // Renew Windows JumpList
                    JumpListHelper.RenewJumpList(await MainWindow.DbContext.EnumerateAllConnectionInfos());
                }
            }).AddTo(Disposable);

            ConnectCommand.Subscribe(async item => await ConfirmConnect(item)).AddTo(Disposable);

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
                    var (result, resultItem) = await Save(item);
                    if (resultItem == null) return; // FAILED
                    item = resultItem; // Replace with the saved item
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
                    // Renew Windows JumpList
                    JumpListHelper.RenewJumpList(await MainWindow.DbContext.EnumerateAllConnectionInfos());
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
            var result = await MainWindow.ShowConfirmationDialog(
                string.Format(Resources.ConnectionInfo_Dialog_Remove_Message, item.ToString()),
                Resources.ConnectionInfo_Dialog_Remove_Title);
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
                await MainWindow.ShowMessageDialog(
                    string.Format(Resources.ConnectionInfo_Dialog_Remove_Error, item.ToString(), ex.Message),
                    Resources.ConnectionInfo_Dialog_Remove_Title);
                return false;
            }
        }

        protected virtual async Task<(bool result, T record)> Save(T item)
        {
            var result = await MainWindow.ShowConfirmationDialog(
                string.Format(Resources.ConnectionInfo_Dialog_Save_Message, item.ToString()),
                Resources.ConnectionInfo_Dialog_Save_Title);
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
                return (res, currentItem);
            }
            catch (Exception ex)
            {
                var logMessage = $"Failed to save {item.ToString()}.";
                var dialogMessage = ex.Message;
                if (ex is System.Data.Entity.Validation.DbEntityValidationException validationEx)
                {
                    var errorMessages = validationEx.EntityValidationErrors
                        .SelectMany(x => x.ValidationErrors)
                        .Select(x => $"{x.PropertyName}: {x.ErrorMessage}")
                        .ToList();
                    if (errorMessages.Any())
                    {
                        logMessage += Newtonsoft.Json.JsonConvert.SerializeObject(errorMessages.ToArray());
                        dialogMessage = string.Join("\r\n", errorMessages.Take(10));
                        if (errorMessages.Count > 10) { dialogMessage += "\r\n..."; }
                    }
                }
                MyLogger.Log(logMessage, ex);
                await MainWindow.ShowMessageDialog(
                    string.Format(Resources.ConnectionInfo_Dialog_Save_Error, item.ToString(), dialogMessage),
                    Resources.ConnectionInfo_Dialog_Save_Title);
                return (false, null);
            }
        }

        protected virtual async Task ConfirmConnect(T item)
        {
            var result = await MainWindow.ShowConfirmationDialog(
                string.Format(Resources.ConnectionInfo_Dialog_Connect_Message, item.ToString()),
                Resources.ConnectionInfo_Dialog_Connect_Title);
            if (!result) return;
            await Connect(item);
        }

        public virtual async Task ConnectById(int id)
        {
            var item = Items.FirstOrDefault(x => x.Id == id);
            if (item == null) throw new System.Collections.Generic.KeyNotFoundException();
            await Connect(item);
        }

        protected virtual async Task Connect(T item)
        {
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
                await MainWindow.ShowMessageDialog(
                    string.Format(Resources.ConnectionInfo_Dialog_Save_Error, item.ToString(), ex.Message),
                    Resources.ConnectionInfo_Dialog_Connect_Title);
            }
        }
    }
}
