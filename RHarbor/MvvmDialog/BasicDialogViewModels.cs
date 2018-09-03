using Reactive.Bindings;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace kenzauros.RHarbor.MvvmDialog
{
    public static class IDialogHostExtensions
    {
        public static Task<bool> ShowDialog(this IDialogHost host, IDialog dialog)
        {
            if (dialog == null) return Task.FromResult(false);
            var waitHandle = new AutoResetEvent(false);
            // ダイアログを閉じる用のイベントに Dialog.Value を null にするメソッドを渡しておく
            dialog.OnClose = () =>
            {
                host.Dialog.Value = null;
                dialog.OnClose = null;
                waitHandle.Set();
            };
            host.Dialog.Value = dialog;
            return Task.Run(() =>
            {
                waitHandle.WaitOne();
                waitHandle.Dispose();
                return dialog.DialogResult;
            });
        }

        public static async Task ShowMessageDialog(this IDialogHost host, string message, string title = null)
        {
            var vm = new MessageDialogViewModel()
            {
                Title = title,
                Message = message
            };
            await host.ShowDialog(vm);
        }

        public static async Task<bool> ShowConfirmationDialog(this IDialogHost host, string message, string title = null)
        {
            var vm = new ConfirmDialogViewModel()
            {
                Title = title,
                Message = message
            };
            return await host.ShowDialog(vm);
        }
    }

    public class MessageDialogViewModel : IDialog
    {
        /// <summary>
        /// ユーザーに表示するメッセージ
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// <see cref="Message"/> プロパティの内容をクリップボードにコピーするコマンドを提供します。
        /// </summary>
        public ReactiveCommand CopyToClipboardCommand { get; } = new ReactiveCommand();

        #region IDialog

        public string Title { get; set; }
        public bool DialogResult { get; set; }
        public AsyncReactiveCommand AcceptCommand { get; set; }
        public AsyncReactiveCommand RejectCommand { get; set; }
        public Func<Task<bool>> Accept { get; set; }
        public Func<Task<bool>> Reject { get; set; }
        public Action OnClose { get; set; }

        #endregion

        static MessageDialogViewModel()
        {
            DialogFactory.Register(typeof(MessageDialogViewModel), typeof(MessageDialogWindow));
        }

        public MessageDialogViewModel()
        {
            this.InitializeAsDialog();
            CopyToClipboardCommand
                 .Subscribe(() => Clipboard.SetText(Message ?? string.Empty));
        }
    }

    public class ConfirmDialogViewModel : MessageDialogViewModel
    {
        static ConfirmDialogViewModel()
        {
            DialogFactory.Register(typeof(ConfirmDialogViewModel), typeof(ConfirmationDialogWindow));
        }
    }
}
