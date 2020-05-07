using Microsoft.Xaml.Behaviors;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace kenzauros.RHarbor.Behaviors
{
    /// <summary>
    /// ウィンドウの「閉じる」機能に関するカスタム動作を指定するビヘイビアです。
    /// </summary>
    /// <seealso cref="http://b.amberfrog.net/post/88379654924/window%E3%81%AE%E9%96%89%E3%81%98%E3%82%8B%E3%83%9C%E3%82%BF%E3%83%B3%E3%82%92%E7%84%A1%E5%8A%B9%E3%81%AB%E3%81%99%E3%82%8Bbehavior"/>
    public class WindowCloseControlBehavior : Behavior<Window>
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("user32.dll")]
        private static extern bool EnableMenuItem(IntPtr hMenu, uint uIDEnableItem, uint uEnable);

        private const uint SC_CLOSE = 0xF060;

        private const uint MF_BYCOMMAND = 0x00000000;
        private const uint MF_GRAYED = 0x00000001;
        private const uint MF_ENABLED = 0x00000000;

        public const int WM_SYSKEYDOWN = 0x0104;
        public const int VK_F4 = 0x73;

        public static readonly DependencyProperty IsCloseEnabledProperty
            = DependencyProperty.Register(
                "IsCloseEnabled", typeof(bool),
                typeof(WindowCloseControlBehavior),
                new PropertyMetadata(true));

        public bool IsCloseEnabled
        {
            get { return (bool)GetValue(IsCloseEnabledProperty); }
            set { SetValue(IsCloseEnabledProperty, value); }
        }

        public static readonly DependencyProperty CloseOnAltF4Property
            = DependencyProperty.Register(
                "CloseOnAltF4",
                typeof(bool),
                typeof(WindowCloseControlBehavior),
                new PropertyMetadata(true));
        public bool CloseOnAltF4
        {
            get { return (bool)GetValue(CloseOnAltF4Property); }
            set { SetValue(CloseOnAltF4Property, value); }
        }

        #region Overrides

        //--- プロパティが変更されたら表示更新
        private static void OnPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (obj is WindowCloseControlBehavior self)
                self.Apply();
        }

        protected override void OnAttached()
        {
            AssociatedObject.SourceInitialized += OnSourceInitialized;
            base.OnAttached();
        }

        protected override void OnDetaching()
        {
            var source = (HwndSource)HwndSource.FromVisual(AssociatedObject);
            source.RemoveHook(HookProcedure);
            AssociatedObject.SourceInitialized -= OnSourceInitialized;
            base.OnDetaching();
        }
        #endregion

        //--- Windowハンドルを取得できるようになったタイミングで初期化
        private void OnSourceInitialized(object sender, EventArgs e)
        {
            Apply();
            var source = (HwndSource)HwndSource.FromVisual(AssociatedObject);
            source.AddHook(HookProcedure);  //--- メッセージフック
        }

        //--- メッセージ処理
        private IntPtr HookProcedure(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            // Alt + F4を無効化
            if (!CloseOnAltF4)
                if (msg == WM_SYSKEYDOWN)
                    if (wParam.ToInt32() == VK_F4)
                        handled = true;

            //--- ok
            return IntPtr.Zero;
        }

        private void Apply()
        {
            if (AssociatedObject == null)
            {
                return;
            }
            var hwnd = new WindowInteropHelper(AssociatedObject).Handle;
            IntPtr hMenu = GetSystemMenu(hwnd, false);
            if (hMenu != IntPtr.Zero)
            {
                if (IsCloseEnabled)
                {
                    EnableMenuItem(hMenu, SC_CLOSE, MF_BYCOMMAND | MF_ENABLED);
                }
                else
                {
                    EnableMenuItem(hMenu, SC_CLOSE, MF_BYCOMMAND | MF_GRAYED);
                }
            }
        }
    }
}
