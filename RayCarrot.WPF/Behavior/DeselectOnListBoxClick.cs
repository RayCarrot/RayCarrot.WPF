using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace RayCarrot.WPF
{
    /// <summary>
    /// Behavior that makes it possible to deselect the selected item in a <see cref="ListBox"/> when clicking outside of the items area
    /// </summary>
    public class DeselectOnListBoxClick : Behavior<ListBox>
    {
        protected override void OnAttached()
        {
            AssociatedObject.MouseDown += UIElement_OnMouseDown;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.MouseDown -= UIElement_OnMouseDown;
        }

        private void UIElement_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                AssociatedObject.SelectedIndex = -1;
        }
    }
}