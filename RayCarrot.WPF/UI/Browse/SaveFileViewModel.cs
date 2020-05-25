namespace RayCarrot.WPF
{
    /// <summary>
    /// The model to use when saving a file
    /// </summary>
    public class SaveFileViewModel : BrowseViewModel
    {
        /// <summary>
        /// The available extensions to save the file to
        /// </summary>
        public string Extensions { get; set; }
    }
}