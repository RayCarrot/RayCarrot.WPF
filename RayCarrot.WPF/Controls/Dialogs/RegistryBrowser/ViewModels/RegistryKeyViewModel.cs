using Microsoft.Win32;
using RayCarrot.CarrotFramework;
using RayCarrot.CarrotFramework.UI;
using RayCarrot.Windows;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace RayCarrot.WPF
{
    /// <summary>
    /// View model for a Registry key item
    /// </summary>
    public class RegistryKeyViewModel : HierarchicalViewModel<RegistryKeyViewModel>
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="fullPath">The full path of the key</param>
        /// <param name="vm">The Registry selection view model</param>
        /// <param name="uifactory">The task factory for the UI</param>
        public RegistryKeyViewModel(string fullPath, RegistrySelectionViewModel vm) : base(RCFWin.RegistryManager.GetSubKeyName(fullPath))
        {
            // Set properties
            FullPath = fullPath;
            VM = vm;
            Name = RCFWin.RegistryManager.GetSubKeyName(fullPath);
        }

        #endregion

        #region Private Fields

        private bool _cacheSubKeys = true;

        private bool _IsExpanded;

        private bool _accessDenied;

        private bool _isEditing;

        #endregion

        #region Public Properties

        /// <summary>
        /// Indicates if sub keys should be cached when the key is not expanded
        /// </summary>
        public virtual bool CacheSubKeys
        {
            get => _cacheSubKeys;
            set
            {
                _cacheSubKeys = value;

                if (!CacheSubKeys && !IsExpanded)
                    ResetCommand.Execute(null);
            }
        }

        /// <summary>
        /// The full path of the key
        /// </summary>
        public virtual string FullPath { get; set; }

        /// <summary>
        /// The name of the key
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// The Registry selection view model
        /// </summary>
        public virtual RegistrySelectionViewModel VM { get; }

        /// <summary>
        /// Indicates if the sub keys have been loaded
        /// </summary>
        public virtual bool LoadedSubKeys { get; set; }

        /// <summary>
        /// Indicates if the key is expanded
        /// </summary>
        public virtual bool IsExpanded
        {
            get => _IsExpanded;
            set => _ = ExpandAsync(value);
        }

        /// <summary>
        /// Indicates if access is denied to the key
        /// </summary>
        public virtual bool AccessDenied
        {
            get => _accessDenied;
            set
            {
                _accessDenied = value;

                if (AccessDenied)
                    Clear();
            }
        }

        /// <summary>
        /// Indicates if the key is selected
        /// </summary>
        public virtual bool IsSelected { get; set; }

        /// <summary>
        /// Indicates if the key name is currently being edited
        /// </summary>
        public virtual bool IsEditing => _isEditing;

        /// <summary>
        /// The edited name
        /// </summary>
        public virtual string EditName { get; set; }

        /// <summary>
        /// Indicates if the key can be edited
        /// </summary>
        public virtual bool CanEditKey => CanAddSubKey && Parent != null;

        /// <summary>
        /// Indicates if sub keys can be added
        /// </summary>
        public virtual bool CanAddSubKey => !VM.BrowseVM.DisableEditing && !AccessDenied;

        #endregion

        #region Public Methods

        /// <summary>
        /// Enables synchronization for this collection
        /// </summary>
        /// <returns>The task</returns>
        public virtual async Task EnableSynchronizationAsync()
        {
            // Enable collection synchronization on the UI thread so we can update sub items on another thread
            await VM.UIFactory.StartNew(() => BindingOperations.EnableCollectionSynchronization(this, FullID));
        }

        /// <summary>
        /// Adds a sub key to the collection
        /// </summary>
        /// <param name="vm">The view model</param>
        public new virtual void Add(RegistryKeyViewModel vm)
        {
            lock (FullID)
                base.Add(vm);
        }

        /// <summary>
        /// Expand the key async
        /// </summary>
        /// <param name="expand">True to expand, false to collapse</param>
        /// <returns>The task</returns>
        public virtual async Task ExpandAsync(bool expand = true)
        {
            // Update backing field
            _IsExpanded = expand;

            // Notify UI
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsExpanded)));

            // Make sure access is not denied
            if (AccessDenied)
                return;

            // Load items if expanded
            if (IsExpanded && !LoadedSubKeys)
                await LoadSubItemsCommand.ExecuteAsync();

            // Reset items if collapsed and set not to cache
            else if (!IsExpanded && !CacheSubKeys)
                await ResetCommand.ExecuteAsync();

            if (VM.AutoSelectOnExpand)
                IsSelected = true;
        }

        /// <summary>
        /// Opens the key in RegEdit
        /// </summary>
        public virtual async Task OpenInRegeditAsync()
        {
            try
            {
                RCFWin.WindowsManager.OpenRegistryPath(FullPath);
            }
            catch (Exception ex)
            {
                ex.HandleError("Opening key in RegEdit");
                await RCFUI.MessageUI.DisplayMessageAsync("The selected key could not be opened in RegEdit", "Error Opening Key", MessageType.Error);
            }
        }

        /// <summary>
        /// Adds the key to the favorites
        /// </summary>
        public virtual async Task AddFavoritesAsync()
        {
            // Get the name
            var result = await new StringInputDialog(new StringInputViewModel()
            {
                Title = "Add to Favorites",
                HeaderText = "Favorite name:",
                StringInput = Name
            }).ShowDialogAsync();

            // Make sure it was not canceled by the user
            if (result.CanceledByUser)
                return;

            // Make sure the name is not empty
            if (result.StringInput.IsNullOrWhiteSpace())
            {
                await RCFUI.MessageUI.DisplayMessageAsync("The name cannot be empty", "Name is not valid", MessageType.Warning);
                return;
            }

            // Make sure the name doesn't already exist
            if (RCFWin.RegistryManager.ValueExists(CommonRegistryPaths.RegeditFavoritesPath, result.StringInput, RegistryView.Default))
            {
                await RCFUI.MessageUI.DisplayMessageAsync("The name already exists", "Name is not valid", MessageType.Warning);
                return;
            }

            try
            {
                // Add the value
                Registry.SetValue(CommonRegistryPaths.RegeditFavoritesPath, result.StringInput, FullPath);

                // Reset the favorites
                await VM.ResetFavoritesAsync();
            }
            catch (Exception ex)
            {
                ex.HandleError("Add registry favorites path");
                await RCFUI.MessageUI.DisplayMessageAsync($"An error occurred saving the name{Environment.NewLine}{ex.Message}", MessageType.Error);
            }
        }

        /// <summary>
        /// Begins renaming the key
        /// </summary>
        public async Task RenameAsync()
        {
            // Make sure the key can be edited
            if (!CanEditKey)
                return;

            // Make sure the key is selected
            VM.SelectedKey = this;

            // Put key into edit mode
            await VM.BeginEditAsync();
        }

        /// <summary>
        /// Sets the <see cref="IsEditing"/> value
        /// </summary>
        /// <param name="isEditing">Indicates if the key is in edit mode</param>
        /// <returns>The task</returns>
        public async Task SetIsEditingAsync(bool isEditing)
        {
            if (isEditing == _isEditing)
                return;

            if (isEditing)
                EditName = Name;

            _isEditing = isEditing;
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsEditing)));

            if (!IsEditing)
                await ProcessEditAsync();
        }

        /// <summary>
        /// Processes the current edit name
        /// </summary>
        public virtual async Task ProcessEditAsync()
        {
            // Make sure the key can be edited
            if (!CanEditKey)
                return;

            // Make sure the name has changed
            if (EditName.Equals(Name, StringComparison.CurrentCultureIgnoreCase))
                return;

            // Make sure the name is not empty
            if (EditName.IsNullOrWhiteSpace())
            {
                await RCFUI.MessageUI.DisplayMessageAsync("The key name can not be blank", "Invalid name", MessageType.Error);
                return;
            }

            // Make sure the name does not contain invalid characters
            if (EditName.Contains("\\"))
            {
                await RCFUI.MessageUI.DisplayMessageAsync(@"The key name can not contain backslashes ('\')", "Invalid name", MessageType.Error);
                return;
            }

            try
            {
                // Check all sub keys for write permission
                if (!GetKey().RunAndDispose(x => x.HasSubKeyTreeWritePermissions()))
                {
                    await RCFUI.MessageUI.DisplayMessageAsync(@"You do not have the required permissions to rename this key", "Error", MessageType.Error);
                    return;
                }

                // Get the parent key
                using (var parent = Parent.GetKey(true))
                {
                    // Move the sub key to new name
                    parent.MoveSubKey(Name, EditName);
                }

                // TODO: Possibly just refresh and expand to this key?

                // Update name
                Name = EditName;

                // Update full path
                FullPath = RCFWin.RegistryManager.CombinePaths(Parent.FullPath, EditName);

                // Reset sub keys
                Reset();

                // Set expanded to false
                IsExpanded = false;

                // Make sure it is selected
                IsSelected = true;

                // Notify UI that the key has changed
                VM.OnPropertyChanged(nameof(VM.SelectedKey));
                VM.OnPropertyChanged(nameof(VM.SelectedKeyFullPath));

                // Refresh the values
                await VM.RefreshValuesAsync();
            }
            catch (Exception ex)
            {
                ex.HandleError("Renaming Registry key", this);
                await RCFUI.MessageUI.DisplayMessageAsync($"Renaming the key failed with the following error message:{Environment.NewLine}{ex.Message}", "Operation Failed", MessageType.Error);
                VM.RefreshCommand.Execute();
            }
        }

        /// <summary>
        /// Adds a new sub key and puts it in edit mode
        /// </summary>
        /// <returns>The task</returns>
        public async Task AddSubKeyAsync()
        {
            // Make sure the key can be edited
            if (!CanEditKey)
                return;

            string name = "New Key #";
            int keyNum = 1;

            try
            {
                while (RCFWin.RegistryManager.KeyExists(RCFWin.RegistryManager.CombinePaths(FullPath, name + keyNum)))
                    keyNum++;
            }
            catch (Exception ex)
            {
                ex.HandleError("Getting new sub key name");
                await RCFUI.MessageUI.DisplayMessageAsync("An unknown error occurred", "Error", MessageType.Error);
                return;
            }

            try
            {
                // Get this key
                using (var key = GetKey(true))
                    // Create the sub key
                    key.CreateSubKey(name + keyNum).Dispose();
            }
            catch (Exception ex)
            {
                ex.HandleError("Creating sub key");
                await RCFUI.MessageUI.DisplayMessageAsync($"The sub key could not be created with the error message of: {Environment.NewLine}{ex.Message}", "Error", MessageType.Error);
                return;
            }

            try
            {
                if (!IsExpanded)
                    await ExpandAsync();

                var vm = new RegistryKeyViewModel(RCFWin.RegistryManager.CombinePaths(FullPath, name + keyNum), VM);
                await vm.EnableSynchronizationAsync();
                Add(vm);
                vm.IsSelected = true;
                await VM.BeginEditAsync();
            }
            catch (Exception ex)
            {
                ex.HandleError("Handling new sub key creation");
                VM.RefreshCommand.Execute();
                return;
            }
        }

        /// <summary>
        /// Gets a new <see cref="RegistryKey"/> for the current key item
        /// </summary>
        /// <param name="writable">True if the key should be writable, otherwise false</param>
        /// <returns>The key</returns>
        public virtual RegistryKey GetKey(bool writable = false) => RCFWin.RegistryManager.GetKeyFromFullPath(FullPath, VM.CurrentRegistryView, writable);

        /// <summary>
        /// Deletes the key
        /// </summary>
        public virtual async Task DeleteKeyAsync()
        {
            // Make sure the key can be edited
            if (!CanEditKey)
                return;

            // Have user confirm deleting key
            if (!await RCFUI.MessageUI.DisplayMessageAsync("Are you sure you want to permanently delete this key and all of its subkeys? This operation can not be undone and may cause system instability.", "Confirm Delete", MessageType.Warning, true))
                return;

            // Check all sub keys for write permission
            if (!GetKey().RunAndDispose(x => x.HasSubKeyTreeWritePermissions()))
            {
                await RCFUI.MessageUI.DisplayMessageAsync(@"You do not have the required permissions to delete this key", "Error", MessageType.Error);
                return;
            }

            try
            {
                // Delete the key
                Parent.GetKey(true).DeleteSubKeyTree(Name);
            }
            catch (Exception ex)
            {
                ex.HandleError("Deleting key");
                await RCFUI.MessageUI.DisplayMessageAsync("The key could not be deleted. Some of its subkeys may have been deleted.", "Operation Failed", MessageType.Error);

                VM.RefreshCommand.Execute();
                return;
            }

            // Remove item from parent
            Parent.Remove(this);

            // Select parent
            VM.SelectedKey = Parent;
        }

        /// <summary>
        /// Copies the full key name to the clipboard
        /// </summary>
        public virtual void CopyKeyName()
        {
            Clipboard.SetText(FullPath);
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Resets the collection of sub keys
        /// </summary>
        protected virtual void Reset()
        {
            // Clear the collection
            Clear();

            try
            {
                // Check if there are any sub keys
                if (GetKey().SubKeyCount > 0)
                    // Add dummy item
                    Add(null);

                // Indicate that access is not denied
                AccessDenied = false;
            }
            catch (Exception ex)
            {
                ex.HandleExpected("Getting sub key count");

                // Indicate that access is denied
                AccessDenied = true;
            }

            // Indicate that the sub keys are no longer loaded
            LoadedSubKeys = false;
        }

        /// <summary>
        /// Resets the collection of sub keys async
        /// </summary>
        /// <returns>The task</returns>
        protected virtual async Task ResetAsync()
        {
            await Task.Run(() => Reset());
        }

        /// <summary>
        /// Loads all sub items
        /// </summary>
        /// <returns>The task</returns>
        protected virtual async Task LoadSubItemsAsync()
        {
            await Task.Run(async () =>
            {
                // Clear the keys
                Clear();

                try
                {
                    // Add the sub keys
                    using (var key = GetKey())
                    {
                        foreach (var subKey in key.GetSubKeyNames())
                        {
                            var vm = new RegistryKeyViewModel(RCFWin.RegistryManager.CombinePaths(FullPath, subKey), VM);
                            await vm.EnableSynchronizationAsync();
                            Add(vm);
                            await vm.ResetCommand.ExecuteAsync();
                        }
                    }

                    // Indicate that access is not denied
                    AccessDenied = false;
                }
                catch (Exception ex)
                {
                    ex.HandleExpected("Getting sub keys");

                    // Indicate that access is denied
                    AccessDenied = true;
                }

                // Indicate that the sub keys have been loaded
                LoadedSubKeys = true;
            });
        }

        #endregion

        #region Commands

        private AsyncRelayCommand _AddFavoritesCommand;

        /// <summary>
        /// Command for adding the key to the favorites
        /// </summary>
        public AsyncRelayCommand AddFavoritesCommand => _AddFavoritesCommand ?? (_AddFavoritesCommand = new AsyncRelayCommand(AddFavoritesAsync));

        private AsyncRelayCommand _OpenInRegeditCommand;

        /// <summary>
        /// Command for opening the key in RegEdit
        /// </summary>
        public AsyncRelayCommand OpenInRegeditCommand => _OpenInRegeditCommand ?? (_OpenInRegeditCommand = new AsyncRelayCommand(OpenInRegeditAsync));

        private AsyncRelayCommand _LoadSubItemsCommand;

        /// <summary>
        /// Command for loading all sub items
        /// </summary>
        public AsyncRelayCommand LoadSubItemsCommand => _LoadSubItemsCommand ?? (_LoadSubItemsCommand = new AsyncRelayCommand(LoadSubItemsAsync));

        private AsyncRelayCommand _ResetCommand;

        /// <summary>
        /// Command for resetting the collection of sub keys
        /// </summary>
        public AsyncRelayCommand ResetCommand => _ResetCommand ?? (_ResetCommand = new AsyncRelayCommand(ResetAsync));

        private AsyncRelayCommand _RenameCommand;

        /// <summary>
        /// Command for renaming the key
        /// </summary>
        public AsyncRelayCommand RenameCommand => _RenameCommand ?? (_RenameCommand = new AsyncRelayCommand(RenameAsync, CanEditKey));

        private AsyncRelayCommand _AddSubKeyCommand;

        /// <summary>
        /// Command for adding a new sub key
        /// </summary>
        public AsyncRelayCommand AddSubKeyCommand => _AddSubKeyCommand ?? (_AddSubKeyCommand = new AsyncRelayCommand(AddSubKeyAsync, CanAddSubKey));

        private AsyncRelayCommand _DeleteCommand;

        /// <summary>
        /// Command for deleting the key
        /// </summary>
        public AsyncRelayCommand DeleteCommand => _DeleteCommand ?? (_DeleteCommand = new AsyncRelayCommand(DeleteKeyAsync, CanEditKey));

        private RelayCommand _CopyKeyNameCommand;

        /// <summary>
        /// Command for copying the key name
        /// </summary>
        public RelayCommand CopyKeyNameCommand => _CopyKeyNameCommand ?? (_CopyKeyNameCommand = new RelayCommand(CopyKeyName));

        #endregion
    }
}