using Microsoft.Win32;
using RayCarrot.CarrotFramework;
using RayCarrot.CarrotFramework.UI;
using RayCarrot.Windows;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RayCarrot.WPF
{
    /// <summary>
    /// Interaction logic for RegistrySelectionDialog.xaml
    /// </summary>
    public partial class RegistrySelectionDialog : DialogVMWindow<RegistrySelectionViewModel, RegistryBrowserResult>
    {
        #region Constructors

        /// <summary>
        /// Creates a new instance of <see cref="RegistrySelectionDialog"/> with default values
        /// </summary>
        /// <param name="vm">The view model</param>
        public RegistrySelectionDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Creates a new instance of <see cref="RegistrySelectionDialog"/> from a browse view model
        /// </summary>
        /// <param name="vm">The view model</param>
        public RegistrySelectionDialog(RegistryBrowserViewModel vm) : base(new RegistrySelectionViewModel(vm))
        {
            InitializeComponent();
        }

        #endregion

        #region Protected Overrides

        /// <summary>
        /// Gets the current result for the dialog
        /// </summary>
        /// <returns>The result</returns>
        protected override RegistryBrowserResult GetResult()
        {
            return ViewModel.Result;
        }

        #endregion

        #region Event Handlers

        private void TextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (sender is TextBox textBox && e.Key == Key.Enter)
                textBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Close the window
            Close();
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            // Update the view model
            ViewModel.SelectedItem = e.NewValue as RegistryKeyViewModel;
        }

        #endregion
    }
}