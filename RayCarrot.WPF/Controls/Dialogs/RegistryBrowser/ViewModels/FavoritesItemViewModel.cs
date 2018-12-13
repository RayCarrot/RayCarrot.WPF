using System.Threading.Tasks;
using RayCarrot.CarrotFramework;

namespace RayCarrot.WPF
{
    /// <summary>
    /// View model for a favorites item in a Registry selection
    /// </summary>
    public class FavoritesItemViewModel : BaseViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="vm">The Registry selection view model</param>
        public FavoritesItemViewModel(RegistrySelectionViewModel vm)
        {
            VM = vm;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The name of the item
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The key path
        /// </summary>
        public string KeyPath { get; set; }

        /// <summary>
        /// The Registry selection view model
        /// </summary>
        public virtual RegistrySelectionViewModel VM { get; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Navigates to this favorites item
        /// </summary>
        public async Task NavigateAsync()
        {
            await VM.ExpandToPathAsync(KeyPath);
        }

        #endregion

        #region Commands

        private AsyncRelayCommand _NavigateCommand;

        /// <summary>
        /// Command for navigating to this favorites item
        /// </summary>
        public AsyncRelayCommand NavigateCommand => _NavigateCommand ?? (_NavigateCommand = new AsyncRelayCommand(NavigateAsync));

        #endregion
    }
}