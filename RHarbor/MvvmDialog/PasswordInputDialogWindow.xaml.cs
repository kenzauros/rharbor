using System.Windows;

namespace kenzauros.RHarbor.MvvmDialog
{
    public partial class PasswordInputDialogWindow : Window
    {
        public PasswordInputDialogWindow()
        {
            InitializeComponent();
            Loaded += Window_Loaded;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DefaultTextBox.Focus();
        }
    }
}
