using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace RayCarrot.WPF
{
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