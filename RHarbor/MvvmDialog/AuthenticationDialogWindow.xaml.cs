using System.Windows;

namespace kenzauros.RHarbor.MvvmDialog
{
    /// <summary>
    /// AuthenticationDialogWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class AuthenticationDialogWindow : Window
    {
        public AuthenticationDialogWindow()
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
