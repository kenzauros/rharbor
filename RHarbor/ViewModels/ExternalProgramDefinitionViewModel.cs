using kenzauros.RHarbor.Models;
using kenzauros.RHarbor.MvvmDialog;
using kenzauros.RHarbor.Properties;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Threading.Tasks;

namespace kenzauros.RHarbor.ViewModels
{
    internal class ExternalProgramDefinitionViewModel : CompositeDisposableViewModelBase
    {
        protected MainWindowViewModel MainWindow => MainWindowViewModel.Singleton;

        public ExternalProgramDefinition Model { get; }
        public AsyncReactiveCommand<SSHConnectionInfo> Command { get; }

        public ExternalProgramDefinitionViewModel(ExternalProgramDefinition model)
        {
            Model = model;
            Command = new();
            Command.Subscribe(Invoke).AddTo(Disposable);
        }

        private async Task Invoke(SSHConnectionInfo info)
        {
            try
            {
                Model.Invoke(info);
            }
            catch (Exception ex)
            {
                await MainWindow.ShowMessageDialog(
                    Resources.ExternalProgramDefinition_Dialog_Invoke_Failed_Message,
                    Resources.ExternalProgramDefinition_Dialog_Invoke_Title);
            }
        }
    }
}
