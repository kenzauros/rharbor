using kenzauros.RHarbor.Models;
using kenzauros.RHarbor.MvvmDialog;
using kenzauros.RHarbor.Properties;
using kenzauros.RHarbor.Utilities;
using Microsoft.Win32;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Security;
using System.Windows;
using System.Windows.Data;

namespace kenzauros.RHarbor.ViewModels
{
    internal class ConnectionInfoManagementViewModel<T> : CompositeDisposableViewModelBase
        where T : class, IConnectionInfo, IPassword, IRewriteable, new()
    {
        protected MainWindowViewModel MainWindow => MainWindowViewModel.Singleton;

        public ObservableCollection<T> Items { get; set; } = new ObservableCollection<T>();

        public ObservableCollection<ConnectionGroup> Groups { get; } = new ObservableCollection<ConnectionGroup>();
        public ReactivePropertySlim<ConnectionGroup> SelectedGroup { get; } = new ReactivePropertySlim<ConnectionGroup>();
        public ReadOnlyReactivePropertySlim<bool> IsGroupSelected { get; }
        public ReactivePropertySlim<string> FilterText { get; } = new ReactivePropertySlim<string>();

        public ReactiveCommand<T> ConnectCommand { get; set; } = new ReactiveCommand<T>();

        public ReactiveCommand StartEditCommand { get; set; }
        public ReactiveCommand ReplicateCommand { get; set; }
        public AsyncReactiveCommand RemoveCommand { get; }
        public ReactiveProperty<T> SelectedItem { get; set; } = new ReactiveProperty<T>();
        public ReactiveProperty<bool> IsItemEditing { get; set; } = new ReactiveProperty<bool>(mode: ReactivePropertyMode.RaiseLatestValueOnSubscribe);
        public ReadOnlyReactiveProperty<bool> IsNotItemEditing { get; set; }
        public ReactiveProperty<T> EditingItem { get; set; } = new ReactiveProperty<T>();
        public ReactiveCommand AddNewItemCommand { get; set; }
        public ReactiveCommand SaveChangesCommand { get; set; }
        public ReactiveCommand DiscardChangesCommand { get; set; }
        public ReadOnlyReactiveProperty<bool> IsItemSelected { get; set; }

        public ReactiveCommand CopyPasswordToClipboardCommand { get; }

        public ReactiveCommand ExportItemCommand { get; protected set; }
        public ReactiveCommand ImportItemCommand { get; protected set; }

        #region Constructors

        public ConnectionInfoManagementViewModel()
        {
            AddNewItemCommand = IsItemEditing.Inverse().ToReactiveCommand();
            AddNewItemCommand.Subscribe(() =>
            {
                SelectedItem.Value = null;
                EditingItem.Value = new T();
                if (IsGroupSelected?.Value == true)
                {
                    EditingItem.Value.GroupName = SelectedGroup.Value?.Name;
                }
                IsItemEditing.Value = true;
            }).AddTo(Disposable);

            ConnectCommand.Subscribe(async item => await ConfirmConnect(item)).AddTo(Disposable);

            IsItemSelected = SelectedItem.Select(x => x != null).ToReadOnlyReactiveProperty();

            IsNotItemEditing = IsItemEditing.Inverse().ToReadOnlyReactiveProperty();

            SelectedItem.Subscribe(x =>
            {
                EditingItem.Value = SelectedItem.Value;
                IsItemEditing.Value = false;
            }).AddTo(Disposable);

            StartEditCommand = IsItemSelected
                .CombineLatest(IsItemEditing.Inverse(), (a, b) => a && b)
                .ToReactiveCommand();
            StartEditCommand.Subscribe(() =>
            {
                EditingItem.Value = SelectedItem.Value.CloneDeep();
                IsItemEditing.Value = true;
            }).AddTo(Disposable);

            ReplicateCommand = IsItemSelected
                .CombineLatest(IsItemEditing.Inverse(), (a, b) => a && b)
                .ToReactiveCommand();
            ReplicateCommand.Subscribe(() =>
            {
                var replicated = SelectedItem.Value.CloneDeep();
                replicated.Id = -1;
                replicated.Name = $"{replicated.Name} - {Resources.ConnectionInfo_Replication_NameSuffix}";
                SelectedItem.Value = null;
                EditingItem.Value = replicated;
                IsItemEditing.Value = true;
            }).AddTo(Disposable);

            RemoveCommand = IsItemSelected
                .CombineLatest(IsItemEditing.Inverse(), (a, b) => a && b)
                .ToAsyncReactiveCommand();
            RemoveCommand.Subscribe(async () =>
            {
                if (await Remove(SelectedItem.Value))
                {
                    Items.Remove(SelectedItem.Value);
                    // Renew Windows JumpList
                    JumpListHelper.RenewJumpList(await MainWindow.DbContext.EnumerateAllConnectionInfos());
                }
            }).AddTo(Disposable);

            DiscardChangesCommand = IsItemEditing.ToReactiveCommand();
            DiscardChangesCommand.Subscribe(() =>
            {
                EditingItem.Value = SelectedItem.Value ?? new T();
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

            // Connection info filterings
            FilterText
                .Throttle(TimeSpan.FromMilliseconds(500))
                .ObserveOnDispatcher()
                .Subscribe(_ => RefreshCollectionView())
                .AddTo(Disposable);
            SelectedGroup
                .ObserveOnDispatcher()
                .Subscribe(_ => RefreshCollectionView())
                .AddTo(Disposable);

            // If any group is selected or not (except for "All")
            IsGroupSelected = SelectedGroup
                .Select(x => x?.Name != AllGroupName)
                .ToReadOnlyReactivePropertySlim()
                .AddTo(Disposable);

            // Group list extraction on connection info events
            Observable.CombineLatest(
                // When Add, Remove or Update
                Items.CollectionChangedAsObservable()
                    .Select(_ => Unit.Default)
                    .StartWith(Unit.Default),
                // When GroupName property in each element changed
                Items.ObserveElementPropertyChanged()
                    .Where(x => x.EventArgs.PropertyName == nameof(ConnectionInfoBase.GroupName))
                    .Select(_ => Unit.Default)
                    .StartWith(Unit.Default)
                )
                .Throttle(TimeSpan.FromMilliseconds(500)) // Once 500 ms
                .ObserveOnDispatcher()
                .Subscribe(_ =>
                {
                    var selectedGroup = SelectedGroup.Value;
                    // Reload group list
                    Groups.Clear();
                    EnumerateGroups().ToList().ForEach(Groups.Add);
                    // Reset selected group
                    SelectedGroup.Value = (selectedGroup is null) ? Groups.FirstOrDefault() : selectedGroup;
                })
                .AddTo(Disposable);

            // Copy password to clipboard
            CopyPasswordToClipboardCommand = SelectedItem
                .Select(x => !string.IsNullOrWhiteSpace(x?.RawPassword))
                .ToReactiveCommand();
            CopyPasswordToClipboardCommand
                .Subscribe(() => Clipboard.SetText(SelectedItem.Value?.RawPassword))
                .AddTo(Disposable);

            // Export item to file
            ExportItemCommand = IsItemSelected
                .CombineLatest(IsItemEditing.Inverse(), (a, b) => a && b)
                .ToReactiveCommand();
            ExportItemCommand
                .Subscribe(async () => await ExportItem(SelectedItem.Value))
                .AddTo(Disposable);

            // Import item from file
            ImportItemCommand = IsItemEditing.Inverse().ToReactiveCommand();
            ImportItemCommand
                .Subscribe(async () => await ImportItem())
                .AddTo(Disposable);
        }

        #endregion

        #region DataGrid View Control (Filter)

        /// <summary>
        /// Special name indicates that the view has to display all of connection infos regardless of the group name.
        /// </summary>
        internal const string AllGroupName = "_____ALL_____";

        /// <summary>
        /// Enumerates <see cref="ConnectionGroup"/>s which have to be shown in the group selector box.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<ConnectionGroup> EnumerateGroups()
        {
            yield return new ConnectionGroup(AllGroupName, Resources.ConnectionGroup_DisplayName_AllGroup);
            foreach (var groupName in Items.Select(x => x.GroupName ?? string.Empty).Distinct().OrderBy(x => x))
            {
                // Treat empty string as null
                var name = string.IsNullOrEmpty(groupName) ? null : groupName;
                var displayName = string.IsNullOrEmpty(name)
                    ? Resources.ConnectionGroup_DisplayName_Unnamed
                    : name;
                yield return new ConnectionGroup(name, displayName);
            }
        }

        /// <summary>
        /// Refreshes <see cref="CollectionViewSource"/> setting for the <see cref="Items"/> with the selected group and input filter text.
        /// </summary>
        private void RefreshCollectionView()
        {
            var collectionView = CollectionViewSource.GetDefaultView(Items);
            var filterText = FilterText.Value;
            var selectedGroup = SelectedGroup.Value;
            var regex = new Regex(filterText ?? "", RegexOptions.IgnoreCase);
            collectionView.Filter = x =>
            {
                var item = (T)x;
                var groupName = item.GroupName;
                return (selectedGroup is null
                        || selectedGroup.Name == AllGroupName
                        || (selectedGroup.Name is null && string.IsNullOrEmpty(groupName))
                        || groupName == selectedGroup.Name)
                    && (string.IsNullOrEmpty(filterText)
                        || regex.IsMatch(item.Name)
                        || regex.IsMatch(item.Host));
            };
            if (collectionView.SortDescriptions.Count == 0)
            {
                collectionView.SortDescriptions.Add(new SortDescription(nameof(IConnectionInfo.Name), ListSortDirection.Ascending));
            }
        }

        #endregion

        protected virtual async Task<bool> Remove(T item)
        {
            var result = await MainWindow.ShowConfirmationDialog(
                string.Format(Resources.ConnectionInfo_Dialog_Remove_Message, item.ToString()),
                Resources.ConnectionInfo_Dialog_Remove_Title);
            if (!result) return false;
            try
            {
                MyLogger.Log($"Removing {item}...");
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
                MyLogger.Log($"Failed to remove {item}.", ex);
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
            ConnectionViewModel<T> conn = null;
            try
            {
                conn = ConnectionViewModel<T>.CreateFromConnectionInfo(item);
                MainWindow.Connections.Collection.Add(conn);
                await conn.Connect();
            }
            catch (Exception ex)
            {
                if (conn != null)
                {
                    MainWindow.Connections.Collection.Remove(conn);
                }
                switch (ex)
                {
                    case OperationCanceledException oce: // User canceled manually
                        MyLogger.Log($"Canceled to connect to {item.ToString()}.");
                        return;
                }
                MyLogger.Log($"Failed to connect to {item.ToString()}.", ex);
                await MainWindow.ShowMessageDialog(
                    string.Format(Resources.ConnectionInfo_Dialog_Connect_Error, item.ToString(), ex.Message),
                    Resources.ConnectionInfo_Dialog_Connect_Title);
            }
        }

        #region Export/Import

        const string ExportFileExtensionSuffix = "info";

        public virtual async Task ExportItem(T value)
        {
            var ext = $"{value.ConnectionType.ToString().ToLower()}{ExportFileExtensionSuffix}";
            var dialog = new SaveFileDialog
            {
                FilterIndex = 1,
                Filter = $"Connection info files (*.{ext})|*.{ext}",
                Title = Resources.ConnectionInfo_Dialog_Export_Title,
                FileName = $"{value.Name}.{ext}",
                OverwritePrompt = true,
            };
            if (dialog.ShowDialog() != true) return; // Break if cancel
            try
            {
                // Password input
                var initialPassword = Membership.GeneratePassword(12, 0);
                var (result, password) = await MainWindow.ShowPasswordInputDialog(
                    Resources.ConnectionInfo_Dialog_Export_PasswordInput_Message,
                    Resources.ConnectionInfo_Dialog_Export_Title,
                    initialPassword
                    );
                if (result == false) return; // Break if cancel

                var filename = dialog.FileName;
                MyLogger.Log($"Exporting \"{value.Name}\" to \"{filename}\"...");

                var data = value.Serialize();
                var key = AesUtil.GenerateKeyFromPassword(password);
                var encrypted = AesUtil.Encrypt(data, key);
                var encoded = Convert.ToBase64String(encrypted);
                File.WriteAllText(filename, encoded);

                MyLogger.Log($"Exported \"{value.Name}\" to \"{filename}\".");

                // Set password to clipboard
                Clipboard.SetText(password);
                await MainWindow.ShowMessageDialog(
                    string.Format(Resources.ConnectionInfo_Dialog_Export_Exported, filename),
                    Resources.ConnectionInfo_Dialog_Export_Title);
                // Open saved dir
                System.Diagnostics.Process.Start(Path.GetDirectoryName(filename));
            }
            catch (Exception ex)
            {
                MyLogger.Log($"Failed to export.", ex);
                await MainWindow.ShowMessageDialog(
                    string.Format(Resources.ConnectionInfo_Dialog_Export_Error, ex.Message),
                    Resources.ConnectionInfo_Dialog_Export_Title);
            }
        }

        public virtual async Task ImportItem()
        {
            var ext = $"{new T().ConnectionType.ToString().ToLower()}{ExportFileExtensionSuffix}";
            var dialog = new OpenFileDialog
            {
                FilterIndex = 1,
                Filter = $"Connection info files (*.{ext})|*.{ext}",
                Title = Resources.ConnectionInfo_Dialog_Import_Title,
                CheckFileExists = true,
            };
            if (dialog.ShowDialog() != true) return; // Break if cancel
            try
            {
                var filename = dialog.FileName;
                MyLogger.Log($"Importing from \"{filename}\"...");

                // Read data from file
                var data = File.ReadAllText(filename);
                // Password input
                var (result, password) = await MainWindow.ShowPasswordInputDialog(
                    Resources.ConnectionInfo_Dialog_Import_PasswordInput_Message,
                    Resources.ConnectionInfo_Dialog_Import_Title,
                    null
                    );
                if (result == false) return; // Break if cancel

                var decoded = Convert.FromBase64String(data);
                var key = AesUtil.GenerateKeyFromPassword(password);
                var decrypted = AesUtil.Decrypt(decoded, key);
                var info = ConnectionInfoBase.Deserialize<T>(decrypted);

                EditingItem.Value = info;
                IsItemEditing.Value = true;

                MyLogger.Log($"Imported \"{info.Name}\" from \"{filename}\".");

                await MainWindow.ShowMessageDialog(
                    string.Format(Resources.ConnectionInfo_Dialog_Import_Imported, filename),
                    Resources.ConnectionInfo_Dialog_Import_Title);
            }
            catch (Exception ex) when (ex is IOException)
            {
                MyLogger.Log($"Failed to import. (IOException", ex);
                await MainWindow.ShowMessageDialog(
                    string.Format(Resources.ConnectionInfo_Dialog_Import_Error_IOException, ex.Message),
                    Resources.ConnectionInfo_Dialog_Import_Title);
            }
            catch (System.Security.Cryptography.CryptographicException ex)
            {
                MyLogger.Log($"Failed to decrypt.", ex);
                await MainWindow.ShowMessageDialog(
                    Resources.ConnectionInfo_Dialog_Import_Error_CryptographicException,
                    Resources.ConnectionInfo_Dialog_Import_Title);
            }
            catch (Exception ex)
            {
                MyLogger.Log($"Failed to import.", ex);
                await MainWindow.ShowMessageDialog(
                    string.Format(Resources.ConnectionInfo_Dialog_Import_Error, ex.Message),
                    Resources.ConnectionInfo_Dialog_Import_Title);
            }
        }


        #endregion

    }
}
