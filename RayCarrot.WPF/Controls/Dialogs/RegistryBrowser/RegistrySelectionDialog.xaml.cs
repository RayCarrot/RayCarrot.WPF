using RayCarrot.CarrotFramework.UI;
using RayCarrot.Windows;
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
        public RegistrySelectionDialog() : this(new RegistryBrowserViewModel())
        {

        }

        /// <summary>
        /// Creates a new instance of <see cref="RegistrySelectionDialog"/> from a browse view model
        /// </summary>
        /// <param name="vm">The view model</param>
        public RegistrySelectionDialog(RegistryBrowserViewModel vm) : base(new RegistrySelectionViewModel(vm))
        {
            InitializeComponent();
            CanceledByUser = true;
        }

        #endregion

        #region Private Methods

        private void AttemptConfirm()
        {
            if (!RCFWin.RegistryManager.KeyExists(ViewModel.SelectedKeyFullPath, ViewModel.CurrentRegistryView))
            {
                RCFUI.MessageUI.DisplayMessage("The selected key could not be found", "Invalid selection", MessageType.Information);
                return;
            }

            if (ViewModel.BrowseVM.BrowseValue && ViewModel.SelectedValue == null)
            {
                RCFUI.MessageUI.DisplayMessage("A value has to be selected", "Invalid selection", MessageType.Information);
                return;
            }

            CanceledByUser = false;
            Close();
        }

        #endregion

        #region Protected Overrides

        /// <summary>
        /// Gets the current result for the dialog
        /// </summary>
        /// <returns>The result</returns>
        protected override RegistryBrowserResult GetResult()
        {
            return new RegistryBrowserResult()
            {
                KeyPath = ViewModel.SelectedKeyFullPath,
                ValueName = ViewModel.SelectedValue?.Name,
                SelectedRegistryView = ViewModel.CurrentRegistryView,
                CanceledByUser = CanceledByUser
            };
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Indicates if the dialog was canceled by the user, default is true
        /// </summary>
        public bool CanceledByUser { get; set; }

        #endregion

        #region Event Handlers

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (sender is TextBox textBox && e.Key == Key.Enter)
            {
                textBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                e.Handled = true;
            }
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Close the window
            Close();
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            // Update the view model
            ViewModel.SelectedKey = e.NewValue as RegistryKeyViewModel;
        }

        private void Continue_Click(object sender, RoutedEventArgs e)
        {
            AttemptConfirm();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            CanceledByUser = true;
            Close();
        }

        private void EditTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            ViewModel.EndEdit();
        }

        private void EditTextBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is bool v && v && sender is TextBox tb)
            {
                tb.Focus();
                tb.SelectAll();
            }
        }

        private void TreeViewItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (ViewModel.DoubleClickToExpand)
                return;

            if (e.LeftButton != MouseButtonState.Pressed)
                return;

            if (sender is FrameworkElement fe && fe.DataContext is RegistryKeyViewModel vm && vm.IsSelected)
            {
                vm.Rename();
                e.Handled = true;
            }
        }

        #endregion
    }
}