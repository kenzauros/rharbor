using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace kenzauros.RHarbor.Views.Controls
{
    internal sealed class DropDownMenuButton : Button
    {
        /// <summary>
        /// Sets or gets a <see cref="ContextMenu"/> which shows as the drop down menu.
        /// </summary>
        public ContextMenu DropDownMenu
        {
            get => GetValue(DropDownMenuProperty) as ContextMenu;
            set => SetValue(DropDownMenuProperty, value);
        }

        /// <summary>
        /// On click
        /// </summary>
        protected override void OnClick()
        {
            if (DropDownMenu == null) { return; }

            DropDownMenu.PlacementTarget = this;
            DropDownMenu.Placement = PlacementMode.Bottom;
            DropDownMenu.IsOpen = !DropDownMenu.IsOpen;
        }

        /// <summary>
        /// Dependency property for <see cref="DropDownMenu"/>
        /// </summary>
        public static readonly DependencyProperty DropDownMenuProperty
            = DependencyProperty.Register(nameof(DropDownMenu), typeof(ContextMenu), typeof(DropDownMenuButton), new UIPropertyMetadata(null));

    }
}
