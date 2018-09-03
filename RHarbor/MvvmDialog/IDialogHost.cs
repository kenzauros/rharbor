using Reactive.Bindings;

namespace kenzauros.RHarbor.MvvmDialog
{
    public interface IDialogHost
    {
        ReactiveProperty<IDialog> Dialog { get; }
    }
}
