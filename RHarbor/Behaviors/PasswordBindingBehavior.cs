using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace kenzauros.RHarbor.Behaviors
{
    internal class PasswordBindingBehavior : Behavior<PasswordBox>
    {
        public string Password
        {
            get => (string)GetValue(PasswordProperty);
            set => SetValue(PasswordProperty, value);
        }

        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.Register(
                nameof(Password),
                typeof(string),
                typeof(PasswordBindingBehavior),
                new PropertyMetadata("", PasswordPropertyChanged));

        private static void PasswordPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not PasswordBindingBehavior behavior
                || behavior.AssociatedObject is not PasswordBox passwordBox
                || e.NewValue is not string newPassword)
            { return; }

            var oldPassword = e.OldValue as string;
            if (newPassword.Equals(oldPassword)) return;
            if (passwordBox.Password == newPassword) return;
            passwordBox.Password = newPassword;
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.PasswordChanged += PasswordBox_PasswordChanged;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.PasswordChanged -= PasswordBox_PasswordChanged;
            base.OnDetaching();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            Password = AssociatedObject.Password;
        }
    }
}
