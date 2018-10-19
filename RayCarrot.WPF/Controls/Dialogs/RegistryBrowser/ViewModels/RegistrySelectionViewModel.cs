using Microsoft.Win32;
using RayCarrot.CarrotFramework;
using RayCarrot.CarrotFramework.UI;
using RayCarrot.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RayCarrot.WPF
{
    /// <summary>
    /// View model for a Registry selection
    /// </summary>
    public class RegistrySelectionViewModel : BaseViewModel
    {
        #region Constructors

        /// <summary>
        /// Creates a new instance of <see cref="RegistrySelectionViewModel"/> with default values
        /// </summary>
        public RegistrySelectionViewModel() : this(new RegistryBrowserViewModel()
        {

        })
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="RegistrySelectionViewModel"/> from a browse view model
        /// </summary>
        /// <param name="browseVM">The view model</param>
        public RegistrySelectionViewModel(RegistryBrowserViewModel browseVM)
        {
            Keys = new ObservableCollection<RegistryKeyViewModel>();
            Favorites = new ObservableCollection<FavoritesItemViewModel>();
            Values = new ObservableCollection<RegistryValueViewModel>();
            BrowseVM = browseVM;

            // Set values from defaults
            CurrentRegistryView = BrowseVM.DefaultRegistryView;
            ShowEmptyDefaultValues = BrowseVM.AllowEmptyDefaultValues;

            // Reset the view with the default path
            string[] pathID = BrowseVM.DefaultKeyPath.Split(RCFWin.RegistryManager.KeySeparatorCharacter);
            _ = UpdateViewAsync(pathID, pathID);
        }

        #endregion

        #region Private Fields

        private RegistryView _currentRegistryView;

        private RegistryKeyViewModel _selectedItem;

        private bool _showEmptyDefaultValues;

        #endregion

        #region Public Properties

        /// <summary>
        /// The browse view model
        /// </summary>
        public virtual RegistryBrowserViewModel BrowseVM { get; }

        /// <summary>
        /// The available keys to select
        /// </summary>
        public virtual ObservableCollection<RegistryKeyViewModel> Keys { get; }

        /// <summary>
        /// The favorites items
        /// </summary>
        public virtual ObservableCollection<FavoritesItemViewModel> Favorites { get; }

        /// <summary>
        /// The available values for the selected key
        /// </summary>
        public virtual ObservableCollection<RegistryValueViewModel> Values { get; }

        /// <summary>
        /// The icon for RegEdit
        /// </summary>
        public virtual Bitmap RegeditIcon
        {
            get
            {
                try
                {
                    return RCFWin.WindowsFileInfoManager.GetIcon(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "regedit.exe"), IconSize.SmallIcon_16);
                }
                catch (Exception ex)
                {
                    ex.HandleUnexpected("Getting regedit icon");
                    return null;
                }
            }
        }

        /// <summary>
        /// The currently selected <see cref="RegistryView"/>
        /// </summary>
        public virtual RegistryView CurrentRegistryView
        {
            get => _currentRegistryView;
            set
            {
                _currentRegistryView = value;

                // Refresh view
                RefreshCommand.Execute();
            }
        }

        /// <summary>
        /// True if nodes should be automatically selected when expanded
        /// </summary>
        public virtual bool AutoSelectOnExpand { get; set; }

        /// <summary>
        /// True if empty default values should be shown
        /// </summary>
        public virtual bool ShowEmptyDefaultValues
        {
            get => _showEmptyDefaultValues;
            set
            {
                if (value == _showEmptyDefaultValues)
                    return;

                _showEmptyDefaultValues = value;

                RefreshValues();
            }
        }

        /// <summary>
        /// The currently selected key
        /// </summary>
        public virtual RegistryKeyViewModel SelectedKey
        {
            get => _selectedItem;
            set
            {
                // Make sure the value has changed
                if (value == _selectedItem)
                    return;

                // Select if not null
                if (value != null)
                    value.IsSelected = true;

                // Update backing field
                _selectedItem = value;

                // Update command status
                OpenInRegeditCommand.CanExecuteCommand = SelectedKey != null;

                // Update the values
                RefreshValues();
            }
        }

        /// <summary>
        /// The full key path of the currently selected item
        /// </summary>
        public virtual string SelectedKeyFullPath
        {
            get => SelectedKey?.FullPath;
            set => _ = ExpandToPathAsync(value);
        }

        /// <summary>
        /// The currently selected value
        /// </summary>
        public virtual RegistryValueViewModel SelectedValue { get; set; }

        #endregion

        #region Protected Flags

        /// <summary>
        /// Indicates if <see cref="ExpandingToPath"/> is currently running
        /// </summary>
        protected bool ExpandingToPath { get; set; }

        /// <summary>
        /// Indicates if <see cref="UpdateViewAsync(string[], string[][])"/> is currently running
        /// </summary>
        protected bool UpdatingView { get; set; }

        /// <summary>
        /// Indicates if <see cref="ResetAsync"/> is currently running
        /// </summary>
        protected bool Resetting { get; set; }

        #endregion

        #region Public Method

        /// <summary>
        /// Resets the keys to the default keys and the favorites
        /// </summary>
        /// <returns>The task</returns>
        public virtual async Task ResetAsync()
        {
            if (Resetting)
                return;

            try
            {
                Resetting = true;

                // Clear collection
                Keys.Clear();

                // Add root keys
                foreach (string baseKey in BrowseVM.AvailableBaseKeys)
                {
                    var vm = new RegistryKeyViewModel(baseKey, this, new TaskFactory(TaskScheduler.FromCurrentSynchronizationContext()));
                    Keys.Add(vm);
                    await vm.ResetCommand.ExecuteAsync();
                }

                if (Keys.Count == 0)
                    throw new Exception("The registry selection can not have 0 root keys");

                // Select the first item
                SelectedKey = Keys[0];

                // Reset the favorites
                ResetFavorites();
            }
            finally
            {
                Resetting = false;
            }
        }

        /// <summary>
        /// Resets the favorites
        /// </summary>
        public virtual void ResetFavorites()
        {
            Favorites.Clear();

            try
            {
                using (var key = RCFWin.RegistryManager.GetKeyFromFullPath(CommonRegistryPaths.RegeditFavoritesPath, RegistryView.Default))
                {
                    foreach (var value in key.GetValues())
                    {
                        Favorites.Add(new FavoritesItemViewModel(this)
                        {
                            Name = value.Name,
                            KeyPath = RCFWin.RegistryManager.NormalizePath(value.Value.ToString())
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                ex.HandleError("Getting Registry favorites");
                RCFUI.MessageUI.DisplayMessage("Could not get the saved favorites", "Unknown error", MessageType.Error);
            }
        }

        /// <summary>
        /// Allows the user to expand to a given key path
        /// </summary>
        /// <returns>The task</returns>
        public virtual async Task ExpandToKeyAsync()
        {
            // Get the key
            var result = new StringInputDialog(new StringInputViewModel()
            {
                Title = "Expand to Key",
                HeaderText = "Enter the key path:",
                StringInput = SelectedKey?.FullPath
            }).ShowDialog();

            // Make sure it was not canceled
            if (result.CanceledByUser)
                return;

            // Expand to the key
            await ExpandToPathAsync(result.StringInput);
        }

        /// <summary>
        /// Expands to the given path
        /// </summary>
        /// <param name="keyPath">The path of the key to expand to</param>
        /// <returns>The task</returns>
        public virtual async Task ExpandToPathAsync(string keyPath)
        {
            if (ExpandingToPath)
                return;

            try
            {
                ExpandingToPath = true;

                if (!RCFWin.RegistryManager.KeyExists(keyPath, CurrentRegistryView))
                    return;

                string[] pathID = keyPath.Split(RCFWin.RegistryManager.KeySeparatorCharacter);
                await UpdateViewAsync(pathID, pathID);
            }
            finally
            {
                ExpandingToPath = false;
            }
        }

        /// <summary>
        /// Updates the view with the specified properties
        /// </summary>
        /// <param name="selectedFullID">The full ID of the key to select</param>
        /// <param name="expandedFullIDs">The full IDs of the keys to expand</param>
        /// <returns>The task</returns>
        public virtual async Task UpdateViewAsync(string[] selectedFullID, params string[][] expandedFullIDs)
        {
            if (Resetting || UpdatingView)
                return;

            try
            {
                UpdatingView = true;

                // Reset the keys
                await ResetAsync();

                // Expand all nodes which match the given paths
                foreach (var path in expandedFullIDs)
                {
                    // Save the last key collection
                    var keys = Keys;

                    // Expand all keys and sub keys
                    for (int i = 0; i < path.Length; i++)
                    {
                        // Get the index
                        int index = keys.FindItemIndex(x => x.ID == path[i]);

                        // Make sure we got an index
                        if (index == -1)
                            break;

                        // Get the key
                        var key = keys[index];

                        // Expand the key if not expanded
                        if (!key.IsExpanded)
                            await key.ExpandAsync();

                        // Set the last node collection
                        keys = key;
                    }
                }

                if (selectedFullID != null)
                {
                    RegistryKeyViewModel lastSelected = null;
                    foreach (var root in Keys)
                    {
                        if (root.FullID.SequenceEqual(selectedFullID))
                        {
                            lastSelected = root;
                            break;
                        }

                        // Find the selected key
                        lastSelected = root.GetAllChildren().FindItem(x => x.FullID.SequenceEqual(selectedFullID));

                        if (lastSelected != null)
                            break;
                    }

                    // Select it if found
                    if (lastSelected != null)
                        SelectedKey = lastSelected;
                }
            }
            finally
            {
                UpdatingView = false;
            }
        }

        /// <summary>
        /// Opens the currently selected key in RegEdit
        /// </summary>
        public virtual void OpenInRegedit()
        {
            if (SelectedKey == null)
            {
                RCFUI.MessageUI.DisplayMessage("No key has been selected", "Error Opening Key", MessageType.Information);
                return;
            }

            SelectedKey.OpenInRegedit();
        }

        /// <summary>
        /// Refreshes the list of values
        /// </summary>
        public virtual void RefreshValues()
        {
            // Clear the values
            Values.Clear();

            // Make sure a key is selected
            if (SelectedKey == null)
                return;

            try
            {
                using (RegistryKey key = RCFWin.RegistryManager.GetKeyFromFullPath(SelectedKeyFullPath, CurrentRegistryView))
                {
                    // Add values
                    Values.AddRange(key.GetValues().Select(x => new RegistryValueViewModel()
                    {
                        Name = x.Name,
                        Data = x.Value,
                        Type = x.ValueKind
                    }));

                    // Add empty default if non has been added and if set to do so
                    if (ShowEmptyDefaultValues && !Values.Any(x => x.IsDefault))
                    {
                        Values.Add(new RegistryValueViewModel()
                        {
                            Name = String.Empty,
                            Type = RegistryValueKind.String
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                ex.HandleExpected("Getting Registry key values");
                RCFUI.MessageUI.DisplayMessage("The Registry key values could not be obtained for the selected key", "Error retrieving values", MessageType.Error);
            }
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Refreshes the key view models
        /// </summary>
        protected virtual async Task RefreshAsync()
        {
            // Save all paths which are expanded
            var expanded = new List<string[]>();
            foreach (var root in Keys)
            {
                if (root.IsExpanded)
                    expanded.Add(root.FullID);

                expanded.AddRange(root.GetAllChildren().Where(x => x?.IsExpanded == true).Select(x => x.FullID));
            }

            // Save the selected path
            var selected = SelectedKey?.FullID;

            // Update view to specified paths
            await UpdateViewAsync(selected, expanded.ToArray());
        }

        #endregion

        #region Commands

        private AsyncRelayCommand _ExpandToKeyCommand;

        /// <summary>
        /// Command for expanding to key
        /// </summary>
        public AsyncRelayCommand ExpandToKeyCommand => _ExpandToKeyCommand ?? (_ExpandToKeyCommand = new AsyncRelayCommand(ExpandToKeyAsync));

        private RelayCommand _OpenInRegeditCommand;

        /// <summary>
        /// Command for opening the selected key in RegEdit
        /// </summary>
        public RelayCommand OpenInRegeditCommand => _OpenInRegeditCommand ?? (_OpenInRegeditCommand = new RelayCommand(OpenInRegedit, SelectedKey != null));

        private AsyncRelayCommand _RefreshCommand;

        /// <summary>
        /// Command for refreshing the key view models
        /// </summary>
        public AsyncRelayCommand RefreshCommand => _RefreshCommand ?? (_RefreshCommand = new AsyncRelayCommand(RefreshAsync));

        #endregion

        #region WIP

        private RelayCommand _BeginEditCommand;

        public RelayCommand BeginEditCommand => _BeginEditCommand ?? (_BeginEditCommand = new RelayCommand(BeginEdit));

        public void BeginEdit()
        {
            // Make sure a key is selected and the selected key has a parent key in the list and it does not have access denied
            if (SelectedKey != null && SelectedKey.FullID.Length > 1 && !SelectedKey.AccessDenied)
                EditingKey = SelectedKey;
        }

        private RelayCommand _EndEditCommand;

        public RelayCommand EndEditCommand => _EndEditCommand ?? (_EndEditCommand = new RelayCommand(EndEdit));

        public void EndEdit()
        {
            EditingKey = null;
        }

        public RegistryKeyViewModel EditingKey
        {
            get => _editingKey;
            set
            {
                if (value == _editingKey)
                    return;

                if (_editingKey != null)
                    _editingKey.IsEditing = false;

                _editingKey = value;

                if (EditingKey != null)
                    EditingKey.IsEditing = true;
            }
        }

        private RegistryKeyViewModel _editingKey;

        #endregion
    }
}