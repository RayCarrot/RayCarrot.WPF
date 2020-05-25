namespace RayCarrot.WPF
{
    /// <summary>
    /// A model to use when browsing for a directory
    /// </summary>
    public class DirectoryBrowserViewModel : BrowseViewModel
    {
        /// <summary>
        /// Enables or disables multi selection option
        /// </summary>
        public bool MultiSelection { get; set; }
    }
}