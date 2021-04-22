using System.Threading.Tasks;

namespace kenzauros.RHarbor.MvvmDialog
{
    internal class PasswordInputDialogViewModel : MessageDialogViewModel
    {
        public string Password { get; set; }

        static PasswordInputDialogViewModel()
        {
            DialogFactory.Register(typeof(PasswordInputDialogViewModel), typeof(PasswordInputDialogWindow));
        }
    }

    internal static class PasswordInputDialogViewModelExtensions
    {
        public static async Task<(bool result, string password)>
            ShowPasswordInputDialog(
                this IDialogHost host,
                string message = null,
                string title = null,
                string initialPassword = null
            )
        {
            var vm = new PasswordInputDialogViewModel()
            {
                Title = title ?? "Password input",
                Message = message ?? "Type your password.",
                Password = initialPassword,
            };
            var result = await host.ShowDialog(vm);
            return (result, vm.Password);
        }
    }
}
