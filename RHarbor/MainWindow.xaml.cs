using kenzauros.RHarbor.ViewModels;
using System;
using System.Windows;

namespace kenzauros.RHarbor
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await (DataContext as MainWindowViewModel).Load();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            (DataContext as MainWindowViewModel).Dispose();
        }
    }
}
