using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;

namespace kenzauros.RHarbor.Views.Controls
{
    /// <summary>
    /// ComboBox that only accepts alphanumeric characters
    /// </summary>
    public class ConnectionParameterKeyComboBox : ComboBox
    {
        readonly static Regex _AcceptableCharRegex = new(@"[^a-zA-Z0-9_]");

        /// <summary>
        /// OnPreviewTextInput
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            if (_AcceptableCharRegex.IsMatch(e.Text))
            {
                e.Handled = true;
            }
            base.OnPreviewTextInput(e);
        }

        /// <summary>
        /// Initializes an instance of <see cref="ConnectionParameterKeyComboBox"/>.
        /// </summary>
        public ConnectionParameterKeyComboBox()
        {
            AddHandler(System.Windows.Controls.Primitives.TextBoxBase.TextChangedEvent,
                      new TextChangedEventHandler(OnTextChanged));
        }

        /// <summary>
        /// OnTextChanged
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            string replaced = _AcceptableCharRegex.Replace(Text, "");
            if (Text != replaced)
            {
                Text = replaced;
                e.Handled = true;
            }
        }
    }
}
