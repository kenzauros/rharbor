using System.Threading.Tasks;

namespace kenzauros.RHarbor.MvvmDialog
{
    internal class AuthenticationDialogViewModel : MessageDialogViewModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public bool SavePassword { get; set; }

        static AuthenticationDialogViewModel()
        {
            DialogFactory.Register(typeof(AuthenticationDialogViewModel), typeof(AuthenticationDialogWindow));
        }
    }

    internal static class AuthenticationDialogViewModelExtensions
    {
        public static async Task<(bool result, string username, string password, bool savePassword)>
            ShowAuthenticationDialog(
            this IDialogHost host,
            string message = null,
            string title = null,
            string username = null,
            bool savePassword = false
            )
        {
            var vm = new AuthenticationDialogViewModel()
            {
                Title = title ?? "Authentication",
                Message = message ?? "Type your authentication information.",
                Username = username,
                SavePassword = savePassword,
            };
            var result = await host.ShowDialog(vm);
            return (result, vm.Username, vm.Password, vm.SavePassword);
        }
    }
}
