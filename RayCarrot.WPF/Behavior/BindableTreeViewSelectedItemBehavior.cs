using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Xaml.Behaviors;

namespace RayCarrot.WPF
{
    /// <summary>
    /// Behavior that makes the <see cref="TreeView.SelectedItem" /> bindable
    /// </summary>
    public class BindableTreeViewSelectedItemBehavior : Behavior<TreeView>
    {
        /// <summary>
        /// Identifies the <see cref="SelectedItem" /> dependency property
        /// </summary>
        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(nameof(SelectedItem), typeof(object), typeof(BindableTreeViewSelectedItemBehavior), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedItemChanged));

        /// <summary>
        /// Gets or sets the selected item of the <see cref="TreeView" /> that this behavior is attached to
        /// </summary>
        public object SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        /// <summary>
        /// Called after the behavior is attached to an AssociatedObject.
        /// </summary>
        protected override void OnAttached()
        {
            AssociatedObject.SelectedItemChanged += OnTreeViewSelectedItemChanged;
        }

        /// <summary>
        /// Called when the behavior is being detached from its AssociatedObject, but before it has
        /// actually occurred.
        /// </summary>
        protected override void OnDetaching()
        {
            if (AssociatedObject != null)
                AssociatedObject.SelectedItemChanged -= OnTreeViewSelectedItemChanged;
        }

        private static Action<int> GetBringIndexIntoView(Panel itemsHostPanel)
        {
            if (!(itemsHostPanel is VirtualizingStackPanel virtualizingPanel))
                return null;

            MethodInfo method = virtualizingPanel.GetType().GetMethod("BringIndexIntoView",
                BindingFlags.Instance | BindingFlags.NonPublic, Type.DefaultBinder, new[] {typeof(int)}, null);

            if (method == null)
                return null;

            return x => method.Invoke(virtualizingPanel, new object[] {x});
        }

        /// <summary>
        /// Recursively search for an item in this subtree.
        /// </summary>
        /// <param name="container">The parent ItemsControl. This can be a TreeView or a TreeViewItem.</param>
        /// <param name="item">The item to search for.</param>
        /// <returns>The TreeViewItem that contains the specified item.</returns>
        private static TreeViewItem GetTreeViewItem(ItemsControl container, object item)
        {
            if (container == null)
                return null;

            if (container.DataContext == item)
                return container as TreeViewItem;

            // Expand the current container
            if (container is TreeViewItem viewItem && !viewItem.IsExpanded)
                viewItem.SetValue(TreeViewItem.IsExpandedProperty, true);

            // Try to generate the ItemsPresenter and the ItemsPanel.
            // by calling ApplyTemplate.  Note that in the 
            // virtualizing case even if the item is marked 
            // expanded we still need to do this step in order to 
            // regenerate the visuals because they may have been virtualized away.
            container.ApplyTemplate();
            ItemsPresenter itemsPresenter = (ItemsPresenter)container.Template.FindName("ItemsHost", container);
            if (itemsPresenter != null)
            {
                itemsPresenter.ApplyTemplate();
            }
            else
            {
                // The Tree template has not named the ItemsPresenter, 
                // so walk the descendants and find the child.
                itemsPresenter = container.GetDescendantByType<ItemsPresenter>();
                if (itemsPresenter == null)
                {
                    container.UpdateLayout();
                    itemsPresenter = container.GetDescendantByType<ItemsPresenter>();
                }
            }

            Panel itemsHostPanel = (Panel) VisualTreeHelper.GetChild(itemsPresenter, 0);

            // Ensure that the generator for this panel has been created.
            _ = itemsHostPanel.Children;

            var bringIndexIntoView = GetBringIndexIntoView(itemsHostPanel);
            for (int i = 0, count = container.Items.Count; i < count; i++)
            {
                TreeViewItem subContainer;
                if (bringIndexIntoView != null)
                {
                    // Bring the item into view so 
                    // that the container will be generated.
                    bringIndexIntoView(i);
                    subContainer = (TreeViewItem) container.ItemContainerGenerator.ContainerFromIndex(i);
                }
                else
                {
                    subContainer = (TreeViewItem) container.ItemContainerGenerator.ContainerFromIndex(i);

                    // Bring the item into view to maintain the same behavior as with a virtualizing panel.
                    subContainer.BringIntoView();
                }

                if (subContainer == null)
                    continue;

                // Search the next level for the object.
                TreeViewItem resultContainer = GetTreeViewItem(subContainer, item);

                if (resultContainer != null)
                    return resultContainer;

                // The object is not under this TreeViewItem
                // so collapse it.
                subContainer.IsExpanded = false;
            }

            return null;
        }

        private static void OnSelectedItemChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is TreeViewItem item)
            {
                item.SetValue(TreeViewItem.IsSelectedProperty, true);
                return;
            }

            BindableTreeViewSelectedItemBehavior behavior = (BindableTreeViewSelectedItemBehavior)sender;

            TreeView treeView = behavior.AssociatedObject;

            if (treeView == null)
                return;

            item = GetTreeViewItem(treeView, e.NewValue);

            if (item != null)
                item.IsSelected = true;
        }

        private void OnTreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            SelectedItem = e.NewValue;
        }
    }
}