using RayCarrot.CarrotFramework;
using RayCarrot.CarrotFramework.IO;
using RayCarrot.CarrotFramework.UI;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace RayCarrot.WPF
{
    /// <summary>
    /// Interaction logic for DriveSelectionDialog.xaml
    /// </summary>
    public partial class DriveSelectionDialog : DialogVMWindow<DriveSelectionViewModel, DriveBrowserResult>
    {
        #region Constructors

        /// <summary>
        /// Creates a new instance of <see cref="DriveSelectionDialog"/> with default values
        /// </summary>
        public DriveSelectionDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Creates a new instance of <see cref="DriveSelectionDialog"/> from a browse view model
        /// </summary>
        /// <param name="vm">The view model</param>
        public DriveSelectionDialog(DriveBrowserViewModel vm) : base(new DriveSelectionViewModel(vm))
        {
            InitializeComponent();
        }

        #endregion
        
        #region Private Methods

        private void AttemptConfirm()
        {
            ViewModel.UpdateReturnValue();

            if (ViewModel.Result.SelectedDrives == null || ViewModel.Result.SelectedDrives.Count < 1 || ViewModel.Result.SelectedDrives.All(x => x == null))
            {
                RCFUI.MessageUI.DisplayMessage("At least one drive has to be selected", "No drive selected", MessageType.Information);
                return;
            }
            if (!ViewModel.Result.SelectedDrives.Select(x => new FileSystemPath(x)).DirectoriesExist())
            {
                RCFUI.MessageUI.DisplayMessage("One or more of the selected drives could not be found", "Invalid selection", MessageType.Information);
                ViewModel.Refresh();
                return;
            }
            if (!ViewModel.BrowseVM.AllowNonReadyDrives && ViewModel.Result.SelectedDrives.Any(x =>
            {
                try
                {
                    return !(new DriveInfo(x).IsReady);
                }
                catch (Exception ex)
                {
                    ex.HandleError("Checking if drive is ready");
                    return true;
                }
            }))
            {
                RCFUI.MessageUI.DisplayMessage("One or more of the selected drives are not ready", "Invalid selection", MessageType.Information);
                ViewModel.Refresh();
                return;
            }

            ViewModel.Result.CanceledByUser = false;
            Close();
        }

        #endregion

        #region Protected Overrides

        /// <summary>
        /// Gets the current result for the dialog
        /// </summary>
        /// <returns>The result</returns>
        protected override DriveBrowserResult GetResult()
        {
            ViewModel.UpdateReturnValue();
            return ViewModel.Result;
        }

        #endregion

        #region Event Handlers

        private void Continue_Click(object sender, RoutedEventArgs e)
        {
            AttemptConfirm();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Result.CanceledByUser = true;
            Close();
        }

        private void DataGrid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                AttemptConfirm();
            }
        }

        #endregion
    }
}