using RayCarrot.IO;

namespace RayCarrot.WPF
{
    /// <summary>
    /// A base class for browse view models
    /// </summary>
    public abstract class BrowseViewModel : UserInputViewModel
    {
        /// <summary>
        /// The default directory to start in when browsing
        /// </summary>
        public FileSystemPath DefaultDirectory { get; set; }

        /// <summary>
        /// The default name of the directory or file to browse for
        /// </summary>
        public string DefaultName { get; set; }
    }
}