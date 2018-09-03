using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Interactivity;

namespace kenzauros.RHarbor.MvvmDialog
{
    public class DialogBehavior : Behavior<UIElement>
    {

        public Object Content
        {
            get { return (Object)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register(
                "Content",
                typeof(Object),
                typeof(DialogBehavior),
                new FrameworkPropertyMetadata(null,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    (depObj, e) =>
                    {
                        var self = depObj as DialogBehavior;
                        if (self._IsAttached)
                        {
                            self.ContentChanged();
                        }
                    }));

        private Window _Dialog;
        private bool _IsAttached;

        protected override void OnAttached()
        {
            _IsAttached = true;

            base.OnAttached();

            if (Content != null)
            {
                ContentChanged();
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
        }

        public void ContentChanged()
        {
            if (_Dialog != null)
            {
                CloseDialog();
            }

            if (Content != null)
            {
                OpenDialog();
            }
        }

        private void OpenDialog()
        {
            var owner = AssociatedObject as Window;
            
            var vm = Content as IDialog;

            _Dialog = DialogFactory.Create(vm.GetType());
            _Dialog.WindowState = WindowState.Normal;
            _Dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            _Dialog.Closed += Closed;
            _Dialog.DataContext = vm;

            Debug.WriteLine($"{DateTime.Now:HH:mm:ss.fff}\tVM: {vm.GetType().Name}, Owner: {owner.GetType().Name}, Dialog: {_Dialog.GetType().Name}");

            Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    _Dialog.Owner = owner;
                    _Dialog.ShowDialog();
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }));
        }

        private void Closed(object sender, EventArgs e)
        {
            if (Content != null) // Window Close Button or Alt+F4 or etc.
            {
                Content = null;
            }
        }

        private void CloseDialog()
        {
            _Dialog.Closed -= Closed;
            _Dialog.Close();
            _Dialog = null;
        }
    }
}
