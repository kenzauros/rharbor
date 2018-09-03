using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Threading.Tasks;

namespace kenzauros.RHarbor.MvvmDialog
{
    /// <summary>
    /// MvvmDialog のダイアログ機能に必要なメンバーを定義します。
    /// </summary>
    public interface IDialog
    {
        string Title { get; set; }
        bool DialogResult { get; set; }
        AsyncReactiveCommand AcceptCommand { get; set; }
        AsyncReactiveCommand RejectCommand { get; set; }
        Func<Task<bool>> Accept { get; set; }
        Func<Task<bool>> Reject { get; set; }
        Action OnClose { get; set; }
    }

    /// <summary>
    /// <see cref="IDialog"/> の拡張子を定義します。
    /// </summary>
    public static class IDialogExtensions
    {
        /// <summary>
        /// <see cref="IDialog"/> の <see cref="RejectCommand"/> を初期化します。
        /// </summary>
        /// <param name="src"></param>
        public static void InitializeAsDialog(this IDialog src)
        {
            src.Accept = () => Task.FromResult(true);
            src.Reject = () => Task.FromResult(true);
            src.AcceptCommand = new AsyncReactiveCommand();
            src.AcceptCommand.Subscribe(async () => { if (await src.Accept()) src.CloseWindow(true); });
            src.RejectCommand = new AsyncReactiveCommand();
            src.RejectCommand.Subscribe(async () => { if (await src.Reject()) src.CloseWindow(false); });
        }

        public static IDisposable SubscribeAsDialogCommand(this AsyncReactiveCommand command, IDialog dialog, bool dialogResult, Func<Task<bool>> onExecute)
        {
            return command.Subscribe(async () => { if (await onExecute()) dialog.CloseWindow(dialogResult); });
        }

        public static void CloseWindow(this IDialog src, bool dialogResult = false)
        {
            src.DialogResult = dialogResult;
            src.OnClose?.Invoke();
        }
    }
}
