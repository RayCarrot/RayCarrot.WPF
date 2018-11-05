using RayCarrot.CarrotFramework;
using System.Collections.Generic;
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
        public static UserLevel GetMinUserLevel(DependencyObject obj) => (UserLevel)obj.GetValue(GetMinUserLevelProperty);

        /// <summary>
        /// Sets the minimum <see cref="UserLevel"/> for a <see cref="DependencyObject"/>
        /// </summary>
        /// <param name="obj">The object to set the property on</param>
        public static void SetMinUserLevel(DependencyObject obj, UserLevel value) => obj.SetValue(GetMinUserLevelProperty, value);

        /// <summary>
        /// The property for the minimum <see cref="UserLevel"/>
        /// </summary>
        public static readonly DependencyProperty GetMinUserLevelProperty = DependencyProperty.RegisterAttached("MinUserLevel", typeof(UserLevel), typeof(UserLevelTag), new PropertyMetadata(UserLevel.Normal, MinUserLevelChanged));

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
        public static void SetBehavior(DependencyObject obj, UserLevelTagBehavior value) => obj.SetValue(BehaviorProperty, value);

        /// <summary>
        /// The property for the requested behavior
        /// </summary>
        public static readonly DependencyProperty BehaviorProperty = DependencyProperty.RegisterAttached("Behavior", typeof(UserLevelTagBehavior), typeof(UserLevelTag), new PropertyMetadata(UserLevelTagBehavior.Collapse, BehaviorChanged));

        #endregion

        #region Private Properties

        /// <summary>
        /// The saved elements to modify when the <see cref="UserLevel"/> changes
        /// </summary>
        private static HashSet<FrameworkElement> Items { get; } = new HashSet<FrameworkElement>();

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

            // Add the UI Element to the list
            if (!Items.Contains(uIElement))
                Items.Add(uIElement);

            // Subscribe to when the current user level changes
            RCF.Data.UserLevelChanged -= RCF_UserLevelChanged;
            RCF.Data.UserLevelChanged += RCF_UserLevelChanged;

            //// Subscribe to when the element is unloaded
            //uIElement.Unloaded -= UIElement_Unloaded;
            //uIElement.Unloaded += UIElement_Unloaded;

            // Refresh the element
            RefreshElement(uIElement);
        }

        // TODO: Find way to remove unused references - such as when a window has closed
        //private static void UIElement_Unloaded(object sender, RoutedEventArgs e)
        //{
        //    if (!(sender is FrameworkElement element))
        //        return;

        //    // Remove the item
        //    Items.Remove(element);

        //    // Unsubscribe the event for the item
        //    element.Unloaded -= UIElement_Unloaded;
        //}

        private static void RCF_UserLevelChanged(object sender, PropertyChangedEventArgs<UserLevel> e)
        {
            // Refresh all elements
            RefreshAllElements();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Refreshes all elements
        /// </summary>
        public static void RefreshAllElements()
        {
            // Update all the elements in the list
            foreach (var element in Items)
                RefreshElement(element);

            RCF.Logger.LogTraceSource($"All elements in the attached property {nameof(UserLevelTag)} had their visibility refreshed based on their user level tags");
        }

        /// <summary>
        /// Refreshes a element
        /// </summary>
        /// <param name="element">The element to refresh</param>
        public static void RefreshElement(FrameworkElement element)
        {
            var b = GetBehavior(element);

            if (b == UserLevelTagBehavior.Collapse)
                element.Visibility = GetMinUserLevel(element) <= RCF.Data.CurrentUserLevel ? Visibility.Visible : Visibility.Collapsed;
            else if (b == UserLevelTagBehavior.Disable)
                element.IsEnabled = GetMinUserLevel(element) <= RCF.Data.CurrentUserLevel;
        }

        #endregion
    }
}