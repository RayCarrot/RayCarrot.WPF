using Microsoft.Win32;
using RayCarrot.CarrotFramework;
using RayCarrot.CarrotFramework.UI;
using RayCarrot.Windows;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
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
        public RegistryKeyViewModel(string fullPath, RegistrySelectionViewModel vm, TaskFactory uifactory) : base(RCFWin.RegistryManager.GetSubKeyName(fullPath))
        {    
            // Set properties
            FullPath = fullPath;
            VM = vm;
            Name = RCFWin.RegistryManager.GetSubKeyName(fullPath);

            // Save the UI thread so we can use it for collection synchronization
            UIFactory = uifactory;

            // Enable collection synchronization on the UI thread so we can update sub items on another thread
            UIFactory.StartNew(() => BindingOperations.EnableCollectionSynchronization(this, FullID)).Wait();
        }

        #endregion

        #region Private Fields

        private bool _cacheSubKeys = true;

        private bool _IsExpanded = false;

        private bool _accessDenied;

        #endregion

        #region Public Properties

        /// <summary>
        /// The task factory for the UI
        /// </summary>
        public virtual TaskFactory UIFactory { get; }

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
        public virtual string FullPath { get; }

        /// <summary>
        /// The name of the key
        /// </summary>
        public virtual string Name { get; }

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

        #endregion

        #region Public Methods

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
        public virtual void OpenInRegedit()
        {
            try
            {
                RCFWin.WindowsManager.OpenRegistryPath(FullPath);
            }
            catch (Exception ex)
            {
                ex.HandleError("Opening key in RegEdit");
                RCFUI.MessageUI.DisplayMessage("The selected key could not be opened in RegEdit", "Error Opening Key", MessageType.Error);
            }
        }

        /// <summary>
        /// Adds the key to the favorites
        /// </summary>
        public virtual void AddFavorites()
        {
            // Get the name
            var result = new StringInputDialog(new StringInputViewModel()
            {
                Title = "Add to Favorites",
                HeaderText = "Favorite name:",
                StringInput = Name
            }).ShowDialog();

            // Make sure it was not canceled by the user
            if (result.CanceledByUser)
                return;

            // Make sure the name is not empty
            if (result.StringInput.IsNullOrWhiteSpace())
            {
                RCFUI.MessageUI.DisplayMessage("The name cannot be empty", "Name is not valid", MessageType.Warning);
                return;
            }

            // Make sure the name doesn't already exist
            if (RCFWin.RegistryManager.ValueExists(CommonRegistryPaths.RegeditFavoritesPath, result.StringInput, RegistryView.Default))
            {
                RCFUI.MessageUI.DisplayMessage("The name already exists", "Name is not valid", MessageType.Warning);
                return;
            }

            try
            {
                // Add the value
                Registry.SetValue(CommonRegistryPaths.RegeditFavoritesPath, result.StringInput, FullPath);

                // Reset the favorites
                VM.ResetFavorites();
            }
            catch (Exception ex)
            {
                ex.HandleError("Add registry favorites path");
                RCFUI.MessageUI.DisplayMessage($"An error occurred saving the name{Environment.NewLine}{ex.Message}", MessageType.Error);
            }
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Resets the collection of sub keys
        /// </summary>
        /// <returns>The task</returns>
        protected virtual async Task ResetAsync()
        {
            await Task.Run(() =>
            {
                // Clear the collection
                Clear();

                try
                {
                    // Check if there are any sub keys
                    if (RCFWin.RegistryManager.GetKeyFromFullPath(FullPath, VM.CurrentRegistryView).SubKeyCount > 0)
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

                // Indicate the the sub keys are no longer loaded
                LoadedSubKeys = false;
            });
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
                    using (var key = RCFWin.RegistryManager.GetKeyFromFullPath(FullPath, VM.CurrentRegistryView))
                    {
                        foreach (var subKey in key.GetSubKeyNames())
                        {
                            var vm = new RegistryKeyViewModel(RCFWin.RegistryManager.CombinePaths(FullPath, subKey), VM, UIFactory);
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

        private RelayCommand _AddFavoritesCommand;

        /// <summary>
        /// Command for adding the key to the favorites
        /// </summary>
        public RelayCommand AddFavoritesCommand => _AddFavoritesCommand ?? (_AddFavoritesCommand = new RelayCommand(AddFavorites));

        private RelayCommand _OpenInRegeditCommand;

        /// <summary>
        /// Command for opening the key in RegEdit
        /// </summary>
        public RelayCommand OpenInRegeditCommand => _OpenInRegeditCommand ?? (_OpenInRegeditCommand = new RelayCommand(OpenInRegedit));

        private AsyncRelayCommand _LoadSubItemsCommand;

        /// <summary>
        /// The command for loading all sub items
        /// </summary>
        public AsyncRelayCommand LoadSubItemsCommand => _LoadSubItemsCommand ?? (_LoadSubItemsCommand = new AsyncRelayCommand(LoadSubItemsAsync));

        private AsyncRelayCommand _ResetCommand;

        /// <summary>
        /// The command for resetting the collection of sub keys
        /// </summary>
        public AsyncRelayCommand ResetCommand => _ResetCommand ?? (_ResetCommand = new AsyncRelayCommand(ResetAsync));

        #endregion
    }
}