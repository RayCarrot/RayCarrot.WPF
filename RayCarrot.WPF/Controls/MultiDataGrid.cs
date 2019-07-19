using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace RayCarrot.WPF
{
    // TODO: Move to behavior
    /// <summary>
    /// A <see cref="DataGrid"/> with support for binding to a list of selected items
    /// </summary>
    public class MultiDataGrid : DataGrid
    {
        #region Constructor

        public MultiDataGrid()
        {
            SelectionChanged += CustomDataGrid_SelectionChanged;
        }

        #endregion

        #region Event Handlers

        private void CustomDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedItemsList = SelectedItems;
        }

        #endregion

        #region Dependency Properties

        public IList SelectedItemsList
        {
            get => (IList)GetValue(SelectedItemsListProperty);
            set => SetValue(SelectedItemsListProperty, value);
        }

        public static readonly DependencyProperty SelectedItemsListProperty = DependencyProperty.Register(nameof(SelectedItemsList), typeof(IList), typeof(MultiDataGrid));

        #endregion
    }
}