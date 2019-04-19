using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RayCarrot.CarrotFramework;
using RayCarrot.Windows.Registry;

namespace RayCarrot.WPF
{
    // TODO: Split into controls (tree, etc.), use binding and allow styles and other usages besides dialog

    /// <summary>
    /// Interaction logic for RegistrySelectionDialog.xaml
    /// </summary>
    public partial class RegistrySelectionDialog : UserControl, IDialogBaseControl<RegistryBrowserViewModel, RegistryBrowserResult>
    {
        #region Constructors

        /// <summary>
        /// Creates a new instance of <see cref="RegistrySelectionDialog"/> with default values
        /// </summary>
        public RegistrySelectionDialog() : this(new RegistryBrowserViewModel()
        {
            Title = "Select a Registry Key"
        })
        {

        }

        /// <summary>
        /// Creates a new instance of <see cref="RegistrySelectionDialog"/> from a browse view model
        /// </summary>
        /// <param name="vm">The view model</param>
        public RegistrySelectionDialog(RegistryBrowserViewModel vm)
        {
            InitializeComponent();

            ViewModel = vm;
            DataContext = new RegistrySelectionViewModel(vm);
            CanceledByUser = true;

            Unloaded += RegistrySelectionDialog_Unloaded;
        }

        #endregion

        #region Private Methods

        private async Task AttemptConfirmAsync()
        {
            if (!RCFWinReg.RegistryManager.KeyExists(RegistrySelectionVM.SelectedKeyFullPath, RegistrySelectionVM.CurrentRegistryView))
            {
                await RCF.MessageUI.DisplayMessageAsync("The selected key could not be found", "Invalid selection", MessageType.Information);
                return;
            }

            if (RegistrySelectionVM.BrowseVM.BrowseValue && RegistrySelectionVM.SelectedValue == null)
            {
                await RCF.MessageUI.DisplayMessageAsync("A value has to be selected", "Invalid selection", MessageType.Information);
                return;
            }

            CanceledByUser = false;
            CloseDialog?.Invoke(this, new EventArgs());
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the current result for the dialog
        /// </summary>
        /// <returns>The result</returns>
        public RegistryBrowserResult GetResult()
        {
            return new RegistryBrowserResult()
            {
                KeyPath = RegistrySelectionVM.SelectedKeyFullPath,
                ValueName = RegistrySelectionVM.SelectedValue?.Name,
                SelectedRegistryView = RegistrySelectionVM.CurrentRegistryView,
                CanceledByUser = CanceledByUser
            };
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {

        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The view model
        /// </summary>
        public RegistryBrowserViewModel ViewModel { get; }

        /// <summary>
        /// The drive selection view model
        /// </summary>
        public RegistrySelectionViewModel RegistrySelectionVM => DataContext as RegistrySelectionViewModel;

        /// <summary>
        /// The dialog content
        /// </summary>
        public object DialogContent => this;

        /// <summary>
        /// Indicates if the dialog should be resizable
        /// </summary>
        public bool Resizable => true;

        /// <summary>
        /// The base size for the dialog
        /// </summary>
        public DialogBaseSize BaseSize => DialogBaseSize.Largest;

        /// <summary>
        /// Indicates if the dialog was canceled by the user, default is true
        /// </summary>
        public bool CanceledByUser { get; set; }

        #endregion

        #region Events

        /// <summary>
        /// Invoke to request the dialog to close
        /// </summary>
        public event EventHandler CloseDialog;

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
            // Close the dialog
            CloseDialog?.Invoke(this, new EventArgs());
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            // Update the view model
            RegistrySelectionVM.SelectedKey = e.NewValue as RegistryKeyViewModel;
        }

        private async void Continue_ClickAsync(object sender, RoutedEventArgs e)
        {
            await AttemptConfirmAsync();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            CanceledByUser = true;
            CloseDialog?.Invoke(this, new EventArgs());
        }

        private async void EditTextBox_LostFocusAsync(object sender, RoutedEventArgs e)
        {
            await RegistrySelectionVM.EndEditAsync();
        }

        private void EditTextBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is bool v && v && sender is TextBox tb)
            {
                tb.Focus();
                tb.SelectAll();
            }
        }

        private void TextBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb && tb.DataContext is RegistryKeyViewModel vm && vm.IsEditing)
            {
                tb.Focus();
                tb.SelectAll();
            }
        }

        private async void TreeViewItem_MouseDownAsync(object sender, MouseButtonEventArgs e)
        {
            if (RegistrySelectionVM.DoubleClickToExpand)
                return;

            if (e.LeftButton != MouseButtonState.Pressed)
                return;

            if (!(sender is FrameworkElement fe) || !(fe.DataContext is RegistryKeyViewModel vm) || !vm.IsSelected)
                return;

            await vm.RenameAsync();
            e.Handled = true;
        }

        private void RegistrySelectionDialog_Unloaded(object sender, RoutedEventArgs e)
        {
            // Save values
            RegistrySelectionVM.SaveSavedValues();
        }

        #endregion
    }
}