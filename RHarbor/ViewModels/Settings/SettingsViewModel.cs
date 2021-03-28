using kenzauros.RHarbor.MvvmDialog;
using kenzauros.RHarbor.Properties;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace kenzauros.RHarbor.ViewModels
{
    internal class SettingsViewModel: CompositeDisposableViewModelBase
    {
        protected MainWindowViewModel MainWindow => MainWindowViewModel.Singleton;

        public ExternalProgramDefinitionSettingsViewModel ExternalProgramDefinitionSettings { get; } = new();

        public AsyncReactiveCommand SaveChangesCommand { get; } = new();

        public SettingsViewModel()
        {
            SaveChangesCommand.Subscribe(async () =>
            {
                try
                {
                    await ExternalProgramDefinitionSettings.SaveChanges();
                }
                catch (System.Exception ex)
                {
                    await MainWindow.ShowMessageDialog(ex.Message,
                        Resources.Settings_Dialog_Save_Title);
                }
            }).AddTo(Disposable);
        }
    }
}
