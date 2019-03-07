using Microsoft.Win32;
using RayCarrot.CarrotFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Nito.AsyncEx;
using RayCarrot.Windows.Registry;
using RayCarrot.Windows.Shell;

namespace RayCarrot.WPF
{
    /// <summary>
    /// The base control for a browse control
    /// </summary>
    public class BrowseControl : UserControl
    {
        #region Constructor

        public BrowseControl()
        {
            BrowseAsyncLock = new AsyncLock();
        }

        #endregion

        #region Methods

        protected virtual bool IsPathValid()
        {
            switch (BrowseType)
            {
                case BrowseTypes.File:
                    return File.Exists(FilePath);

                case BrowseTypes.Directory:
                    return Directory.Exists(FilePath);

                case BrowseTypes.Drive:
                    FileSystemPath path = FilePath;
                    return path.DirectoryExists && path.IsDirectoryRoot();

                case BrowseTypes.RegistryKey:
                    return RCFWinReg.RegistryManager.KeyExists(FilePath, SelectedRegistryView);

                default:
                    return false;
            }
        }

        protected virtual bool AllowFileDragDrop()
        {
            return BrowseType == BrowseTypes.Directory || BrowseType == BrowseTypes.File || BrowseType == BrowseTypes.Drive;
        }

        protected virtual async Task BrowseAsync()
        {
            using (await BrowseAsyncLock.LockAsync())
            {
                switch (BrowseType)
                {
                    case BrowseTypes.File:
                        var fileResult = await RCF.BrowseUI.BrowseFileAsync(new FileBrowserViewModel()
                        {
                            Title = "Select a file",
                            DefaultDirectory = IsPathValid() ? new FileSystemPath(FilePath).Parent.FullPath : InitialDirectory,
                            DefaultName = UseCurrentPathAsDefaultDirectoryIfValid && IsPathValid() ? new FileSystemPath(FilePath).Name : String.Empty,
                            ExtensionFilter = FileFilter
                        });

                        if (fileResult.CanceledByUser)
                            return;

                        FilePath = fileResult.SelectedFile;

                        break;

                    case BrowseTypes.Directory:
                        var dirResult = await RCF.BrowseUI.BrowseDirectoryAsync(new DirectoryBrowserViewModel()
                        {
                            Title = "Select a file",
                            DefaultDirectory = UseCurrentPathAsDefaultDirectoryIfValid && IsPathValid() ? new FileSystemPath(FilePath).FullPath : InitialDirectory,
                            DefaultName = IsPathValid() ? new FileSystemPath(FilePath).Name : String.Empty
                        });

                        if (dirResult.CanceledByUser)
                            return;

                        FilePath = dirResult.SelectedDirectory;

                        break;

                    case BrowseTypes.RegistryKey:
                        var keyResult = await RCFWinReg.RegistryBrowseUIManager.BrowseRegistryKeyAsync(new RegistryBrowserViewModel()
                        {
                            Title = "Select a Registry key",
                            AllowCustomRegistryView = AllowCustomRegistryView,
                            DefaultRegistryView = SelectedRegistryView,
                            BrowseValue = false,
                            DefaultKeyPath = UseCurrentPathAsDefaultDirectoryIfValid && IsPathValid() ? FilePath : InitialDirectory,
                        });

                        if (keyResult.CanceledByUser)
                            return;

                        FilePath = keyResult.KeyPath;
                        SelectedRegistryView = keyResult.SelectedRegistryView;
                        break;

                    case BrowseTypes.Drive:
                        var driveResult = await RCF.BrowseUI.BrowseDriveAsync(new DriveBrowserViewModel()
                        {
                            Title = "Select a drive",
                            DefaultDirectory = UseCurrentPathAsDefaultDirectoryIfValid && IsPathValid() ? new FileSystemPath(FilePath).FullPath : InitialDirectory,
                            MultiSelection = false,
                            AllowedTypes = AllowedDriveTypes,
                            AllowNonReadyDrives = AllowNonReadyDrives
                        });

                        if (driveResult.CanceledByUser)
                            return;

                        FilePath = driveResult.SelectedDrive;
                        break;

                    default:
                        throw new ArgumentException("The specified browse type is not valid");
                }

                MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
        }

        protected virtual async Task OpenLocationAsync()
        {
            if (!IsPathValid())
            {
                await RCF.MessageUI.DisplayMessageAsync($"The path {FilePath} does not exist", "Path not found", MessageType.Error);
                return;
            }

            try
            {
                switch (BrowseType)
                {
                    case BrowseTypes.File:
                    case BrowseTypes.Directory:
                    case BrowseTypes.Drive:
                        WindowsHelpers.OpenExplorerPath(FilePath);
                        break;

                    case BrowseTypes.RegistryKey:
                        WindowsHelpers.OpenRegistryPath(FilePath);
                        break;
                }
            }
            catch (Exception ex)
            {
                ex.HandleUnexpected("Opening browse location");
            }
        }

        protected virtual void FileDrop(DragEventArgs e)
        {
            if (!AllowFileDragDrop() || !e.Data.GetDataPresent(DataFormats.FileDrop))
                return;

            // Get the path
            FileSystemPath filePath = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];

            // Get the the target if it's a shortcut
            if (filePath.FullPath.EndsWith(".lnk"))
                filePath = WindowsHelpers.GetShortCutTarget(filePath);

            // Set the path
            FilePath = filePath;
        }

        #endregion

        #region Private Properties

        private AsyncLock BrowseAsyncLock { get; }

        #endregion

        #region Dependency Properties

        #region FileFilter

        /// <summary>
        /// The file filter to use when browsing
        /// </summary>
        public string FileFilter
        {
            get => (string)GetValue(FileFilterProperty);
            set => SetValue(FileFilterProperty, value);
        }

        public static readonly DependencyProperty FileFilterProperty =
            DependencyProperty.Register(nameof(FileFilter), typeof(string), typeof(BrowseControl),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        #endregion

        #region InitialDirectory

        /// <summary>
        /// The default directory when browsing
        /// </summary>
        public string InitialDirectory
        {
            get => (string)GetValue(InitialFileDirectoryProperty);
            set => SetValue(InitialFileDirectoryProperty, value);
        }

        public static readonly DependencyProperty InitialFileDirectoryProperty =
            DependencyProperty.Register(nameof(InitialDirectory), typeof(string), typeof(BrowseControl), 
                new FrameworkPropertyMetadata(Environment.GetFolderPath(Environment.SpecialFolder.MyComputer), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        #endregion

        #region PathValidation

        /// <summary>
        /// The validation rule to use for the path
        /// </summary>
        public BrowseValidationRules PathValidation
        {
            get => (BrowseValidationRules)GetValue(RequireTextProperty);
            set => SetValue(RequireTextProperty, value);
        }

        public static readonly DependencyProperty RequireTextProperty = 
            DependencyProperty.Register(nameof(PathValidation), typeof(BrowseValidationRules), typeof(BrowseControl), 
                new FrameworkPropertyMetadata(BrowseValidationRules.None, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        #endregion

        #region FilePath

        /// <summary>
        /// The file path
        /// </summary>
        public string FilePath
        {
            get => (string)GetValue(FilePathProperty);
            set => SetValue(FilePathProperty, value);
        }

        public static readonly DependencyProperty FilePathProperty =
            DependencyProperty.Register(nameof(FilePath), typeof(string), typeof(BrowseControl), 
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        #endregion

        #region FilePathToolTip

        /// <summary>
        /// The tool tip to use for the file path text box
        /// </summary>
        public string FilePathToolTip
        {
            get => (string)GetValue(FilePathToolTipProperty);
            set => SetValue(FilePathToolTipProperty, value);
        }

        public static readonly DependencyProperty FilePathToolTipProperty = 
            DependencyProperty.Register(nameof(FilePathToolTip), typeof(string), typeof(BrowseControl),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        #endregion

        #region BrowseType

        /// <summary>
        /// The type of path to browse for
        /// </summary>
        public BrowseTypes BrowseType
        {
            get => (BrowseTypes)GetValue(BrowseTypeProperty);
            set => SetValue(BrowseTypeProperty, value);
        }

        public static readonly DependencyProperty BrowseTypeProperty =
            DependencyProperty.Register(nameof(BrowseType), typeof(BrowseTypes), typeof(BrowseControl),
                new FrameworkPropertyMetadata(BrowseTypes.File, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        #endregion

        #region AllowedDriveTypes

        /// <summary>
        /// The allowed drive types
        /// </summary>
        public IEnumerable<DriveType> AllowedDriveTypes
        {
            get => (IEnumerable<DriveType>)GetValue(AllowedDriveTypesProperty);
            set => SetValue(AllowedDriveTypesProperty, value);
        }

        public static readonly DependencyProperty AllowedDriveTypesProperty = 
            DependencyProperty.Register(nameof(AllowedDriveTypes), typeof(IEnumerable<DriveType>), typeof(BrowseControl),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        #endregion

        #region SelectedRegistryView

        /// <summary>
        /// The selected <see cref="RegistryView"/>
        /// </summary>
        public RegistryView SelectedRegistryView
        {
            get => (RegistryView)GetValue(SelectedRegistryViewProperty);
            set => SetValue(SelectedRegistryViewProperty, value);
        }

        public static readonly DependencyProperty SelectedRegistryViewProperty =
            DependencyProperty.Register(nameof(SelectedRegistryView), typeof(RegistryView), typeof(BrowseBox),
                new FrameworkPropertyMetadata(RegistryView.Default, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        #endregion

        #region UseCurrentPathAsDefaultDirectoryIfValid

        /// <summary>
        /// True if the current path should be used as the default directory if it is valid
        /// </summary>
        public bool UseCurrentPathAsDefaultDirectoryIfValid
        {
            get => (bool)GetValue(UseCurrentPathAsDefaultDirectoryIfValidProperty);
            set => SetValue(UseCurrentPathAsDefaultDirectoryIfValidProperty, value);
        }

        public static readonly DependencyProperty UseCurrentPathAsDefaultDirectoryIfValidProperty = 
            DependencyProperty.Register(nameof(UseCurrentPathAsDefaultDirectoryIfValid), typeof(bool), typeof(BrowseControl),
                new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        #endregion

        #region CanBrowse

        /// <summary>
        /// True if the user can browse for a path
        /// </summary>
        public bool CanBrowse
        {
            get => (bool)GetValue(CanBrowseProperty);
            set => SetValue(CanBrowseProperty, value);
        }

        public static readonly DependencyProperty CanBrowseProperty = 
            DependencyProperty.Register(nameof(CanBrowse), typeof(bool), typeof(BrowseControl),
                new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        #endregion

        #region AllowNonReadyDrives

        /// <summary>
        /// True if non-ready drives are allowed to be selected, false if not
        /// </summary>
        public bool AllowNonReadyDrives
        {
            get => (bool)GetValue(AllowNonReadyDrivesProperty);
            set => SetValue(AllowNonReadyDrivesProperty, value);
        }

        public static readonly DependencyProperty AllowNonReadyDrivesProperty = 
            DependencyProperty.Register(nameof(AllowNonReadyDrives), typeof(bool), typeof(BrowseControl),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        #endregion

        #region AllowNonReadyDrives

        /// <summary>
        /// True if the user can change the <see cref="RegistryView"/>, false if not
        /// </summary>
        public bool AllowCustomRegistryView
        {
            get => (bool)GetValue(AllowCustomRegistryViewProperty);
            set => SetValue(AllowCustomRegistryViewProperty, value);
        }

        public static readonly DependencyProperty AllowCustomRegistryViewProperty = 
            DependencyProperty.Register(nameof(AllowCustomRegistryView), typeof(bool), typeof(BrowseControl),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        #endregion

        #endregion
    }
}