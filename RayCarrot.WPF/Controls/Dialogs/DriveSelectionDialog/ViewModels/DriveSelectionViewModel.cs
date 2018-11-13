using ByteSizeLib;
using RayCarrot.CarrotFramework;
using RayCarrot.CarrotFramework.UI;
using RayCarrot.Windows;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace RayCarrot.WPF
{
    /// <summary>
    /// View model for a drive selection
    /// </summary>
    public class DriveSelectionViewModel : BaseViewModel
    {
        #region Constructor

        /// <summary>
        /// Creates a new instance of <see cref="DriveSelectionViewModel"/> with default values
        /// </summary>
        public DriveSelectionViewModel()
        {
            BrowseVM = new DriveBrowserViewModel()
            {
                AllowedTypes = new DriveType[]
                {
                    DriveType.CDRom,
                    DriveType.Fixed,
                    DriveType.Network,
                    DriveType.Removable
                },
                AllowNonReadyDrives = false,
                MultiSelection = false,
                Title = "Select a drive"
            };
            Setup();
            Drives = new ObservableCollection<DriveViewModel>();
        }

        /// <summary>
        /// Creates a new instance of <see cref="DriveSelectionViewModel"/> with a browse view model
        /// </summary>
        public DriveSelectionViewModel(DriveBrowserViewModel browseVM)
        {
            BrowseVM = browseVM;
            Setup();
            Drives = new ObservableCollection<DriveViewModel>();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The browse view model
        /// </summary>
        public DriveBrowserViewModel BrowseVM { get; }

        /// <summary>
        /// The currently available drives
        /// </summary>
        public ObservableCollection<DriveViewModel> Drives { get; }

        /// <summary>
        /// The current result
        /// </summary>
        public DriveBrowserResult Result { get; set; }

        /// <summary>
        /// The currently selected item
        /// </summary>
        public DriveViewModel SelectedItem { get; set; }

        /// <summary>
        /// The currently selected items
        /// </summary>
        public IList SelectedItems { get; set; }

        #endregion

        #region Private Methods

        /// <summary>
        /// Sets up the view model
        /// </summary>
        private void Setup()
        {
            Result = new DriveBrowserResult()
            {
                CanceledByUser = true
            };
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Updates the return value
        /// </summary>
        public void UpdateReturnValue()
        {
            Result.SelectedDrive = SelectedItem?.Path;
            Result.SelectedDrives = SelectedItems?.Cast<DriveViewModel>().Select(x => x?.Path.FullPath).ToList();
        }

        /// <summary>
        /// Refreshes the available drives
        /// </summary>
        public async Task RefreshAsync()
        {
            Drives.Clear();

            try
            {
                var drives = DriveInfo.GetDrives();
                foreach (var drive in drives)
                {
                    if (BrowseVM.AllowedTypes != null && !BrowseVM.AllowedTypes.Contains(drive.DriveType))
                        continue;

                    Bitmap icon = null;
                    string label = null;
                    string path = null;
                    string format = null;
                    ByteSize? freeSpace = null;
                    ByteSize? totalSize = null;
                    DriveType? type = null;
                    bool? ready = null;

                    try
                    {
                        label = drive.VolumeLabel;
                    }
                    catch (Exception ex)
                    {
                        ex.HandleExpected("Getting drive label");
                    }

                    try
                    {
                        path = drive.Name;

                        try
                        {
                            icon = RCFWin.WindowsFileInfoManager.GetIcon(path, IconSize.SmallIcon_16);
                        }
                        catch (Exception ex)
                        {
                            ex.HandleExpected("Getting drive icon");
                        }
                    }
                    catch (Exception ex)
                    {
                        ex.HandleExpected("Getting drive name");
                        continue;
                    }

                    try
                    {
                        format = drive.DriveFormat;
                    }
                    catch (Exception ex)
                    {
                        ex.HandleExpected("Getting drive format");
                    }

                    try
                    {
                        freeSpace = new ByteSize(drive.TotalFreeSpace);
                    }
                    catch (Exception ex)
                    {
                        ex.HandleExpected("Getting drive freeSpace");
                    }

                    try
                    {
                        totalSize = new ByteSize(drive.TotalSize);
                    }
                    catch (Exception ex)
                    {
                        ex.HandleExpected("Getting drive totalSize");
                    }

                    try
                    {
                        type = drive.DriveType;
                    }
                    catch (Exception ex)
                    {
                        ex.HandleExpected("Getting drive type");
                    }

                    try
                    {
                        ready = drive.IsReady;
                        if (!drive.IsReady && !BrowseVM.AllowNonReadyDrives)
                            continue;
                    }
                    catch (Exception ex)
                    {
                        ex.HandleExpected("Getting drive ready");
                        if (!BrowseVM.AllowNonReadyDrives)
                            continue;
                    }

                    // Create the view model
                    var vm = new DriveViewModel()
                    {
                        Path = path,
                        Icon = icon,
                        Format = format,
                        Label = label,
                        Type = type,
                        FreeSpace = freeSpace,
                        TotalSize = totalSize,
                        IsReady = ready
                    };

                    Drives.Add(vm);
                }
            }
            catch (Exception ex)
            {
                ex.HandleUnexpected("Getting drives");
                await RCFUI.MessageUI.DisplayMessageAsync("An error occurred getting the drives", "Error", MessageType.Error);
            }
        }

        #endregion

        #region Commands

        private ICommand _RefreshCommand;

        /// <summary>
        /// A command for <see cref="Refresh"/>
        /// </summary>
        public ICommand RefreshCommand => _RefreshCommand ?? (_RefreshCommand = new AsyncRelayCommand(RefreshAsync));

        #endregion
    }
}