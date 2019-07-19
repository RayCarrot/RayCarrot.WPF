using RayCarrot.CarrotFramework.Abstractions;
using System;
using System.ComponentModel;
using System.Windows;

namespace RayCarrot.WPF
{
    /// <summary>
    /// Provides attached properties for tagging <see cref="FrameworkElement"/> objects with a minimum <see cref="UserLevel"/>
    /// </summary>
    public static class UserLevelTag
    {
        #region MinUserLevel

        /// <summary>
        /// Gets the minimum <see cref="UserLevel"/> from a <see cref="DependencyObject"/>
        /// </summary>
        /// <param name="obj">The object to get the property from</param>
        /// <returns>The property</returns>
        public static UserLevel GetMinUserLevel(DependencyObject obj) => (UserLevel)obj.GetValue(MinUserLevelProperty);

        /// <summary>
        /// Sets the minimum <see cref="UserLevel"/> for a <see cref="DependencyObject"/>
        /// </summary>
        /// <param name="obj">The object to set the property on</param>
        /// <param name="value">The property to set</param>
        public static void SetMinUserLevel(DependencyObject obj, UserLevel value) => obj.SetValue(MinUserLevelProperty, value);

        /// <summary>
        /// The property for the minimum <see cref="UserLevel"/>
        /// </summary>
        public static readonly DependencyProperty MinUserLevelProperty = DependencyProperty.RegisterAttached("MinUserLevel", typeof(UserLevel), typeof(UserLevelTag), new PropertyMetadata(UserLevel.Normal, MinUserLevelChanged));

        #endregion

        #region Behavior

        /// <summary>
        /// Gets the requested behavior from a <see cref="DependencyObject"/>
        /// </summary>
        /// <param name="obj">The object to get the property from</param>
        /// <returns>The property</returns>
        public static UserLevelTagBehavior GetBehavior(DependencyObject obj) => (UserLevelTagBehavior)obj.GetValue(BehaviorProperty);

        /// <summary>
        /// Sets the requested behavior for a <see cref="DependencyObject"/>
        /// </summary>
        /// <param name="obj">The object to set the property on</param>
        /// <param name="value">The property to set</param>
        public static void SetBehavior(DependencyObject obj, UserLevelTagBehavior value) => obj.SetValue(BehaviorProperty, value);

        /// <summary>
        /// The property for the requested behavior
        /// </summary>
        public static readonly DependencyProperty BehaviorProperty = DependencyProperty.RegisterAttached("Behavior", typeof(UserLevelTagBehavior), typeof(UserLevelTag), new PropertyMetadata(UserLevelTagBehavior.Collapse, BehaviorChanged));

        #endregion

        #region Private Event Handlers

        private static void BehaviorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Return if in design mode
            if (DesignerProperties.GetIsInDesignMode(d))
                return;

            // Make sure we've got a UI Element
            if (!(d is FrameworkElement uIElement))
                return;

            // Revert values to default
            uIElement.IsEnabled = true;
            uIElement.Visibility = Visibility.Visible;

            // Refresh the element
            RefreshElement(uIElement);
        }

        private static void MinUserLevelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Return if in design mode
            if (DesignerProperties.GetIsInDesignMode(d))
                return;

            // Make sure we've got a UI Element
            if (!(d is FrameworkElement uIElement))
                return;

            // Create a weak reference
            var element = new WeakReference<FrameworkElement>(uIElement);

            // Subscribe to when the current user level changes
            RCFCore.Data.UserLevelChanged += RefreshItem;

            // Refresh the element
            RefreshElement(uIElement);

            void RefreshItem(object ss, EventArgs ee)
            {
                if (element.TryGetTarget(out FrameworkElement ue))
                    RefreshElement(ue);
                else
                    RCFCore.Data.UserLevelChanged -= RefreshItem;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Refreshes a element
        /// </summary>
        /// <param name="element">The element to refresh</param>
        public static void RefreshElement(FrameworkElement element)
        {
            var b = GetBehavior(element);

            if (b == UserLevelTagBehavior.Collapse)
                element.Visibility = GetMinUserLevel(element) <= RCFCore.Data.CurrentUserLevel ? Visibility.Visible : Visibility.Collapsed;
            else if (b == UserLevelTagBehavior.Disable)
                element.IsEnabled = GetMinUserLevel(element) <= RCFCore.Data.CurrentUserLevel;
        }

        #endregion
    }
}