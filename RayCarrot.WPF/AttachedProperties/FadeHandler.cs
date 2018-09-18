using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace RayCarrot.WPF
{
    /// <summary>
    /// An attached property for fading in a control when the mouse enters the specified parent control
    /// </summary>
    public class FadeHandlerProperty : BaseAttachedProperty<FadeHandlerProperty, Control>
    {
        #region Public Overrides

        public override void OnValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var control = (sender as Control);

            Control = GetValue(sender);

            if (FirstRun)
            {
                Control.Opacity = 0;
                FirstRun = false;
            }

            // TODO: Unsubscribe old events when value changes
            control.MouseEnter += (object ss, MouseEventArgs ee) => FadeControl(true);

            control.MouseLeave += (object ss, MouseEventArgs ee) => FadeControl(false);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Fades the control in or out
        /// </summary>
        /// <param name="fadeIn">True if the control should fade in, false if it should fade out</param>
        private void FadeControl(bool fadeIn)
        {
            Storyboard storyboard = new Storyboard();
            DoubleAnimation doubleAnimation = new DoubleAnimation()
            {
                To = fadeIn ? 1 : 0,
                Duration = new Duration(TimeSpan.FromSeconds(0.3)),
            };

            Storyboard.SetTarget(doubleAnimation, Control);
            Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath(UIElement.OpacityProperty));

            storyboard.Children.Add(doubleAnimation);

            storyboard.Begin();
        }

        #endregion

        #region Private Properties

        /// <summary>
        /// Indicates if it's the first run
        /// </summary>
        private bool FirstRun { get; set; } = true;

        /// <summary>
        /// The control to fade in and out
        /// </summary>
        private Control Control { get; set; }

        #endregion
    }
}