using Reactive.Bindings;
using System;
using System.Collections.ObjectModel;

namespace kenzauros.RHarbor.ViewModels
{
    internal interface IConnectionViewModel : IDisposable
    {
        ReactiveProperty<bool> IsConnecting { get; set; }
        ReactiveProperty<bool> IsConnected { get; set; } 
        ReactiveProperty<string> ConnectionName { get; set; } 
        ReactiveProperty<string> Host { get; set; }
        ReactiveProperty<int?> Port { get; set; }
        ReactiveProperty<string> DisplayText { get; set; }

        IConnectionViewModel Parent { get; set; }
        ObservableCollection<IConnectionViewModel> Children { get; set; }

        AsyncReactiveCommand DisconnectCommand { get; set; }

        /// <summary>
        /// Whether this connection is root or not.
        /// </summary>
        ReactiveProperty<bool> IsRoot { get; }

    }
}
