using RayCarrot.Windows;

namespace RayCarrot.WPF
{
    /// <summary>
    /// Common Registry paths for <see cref="RayCarrot.WPF"/>
    /// </summary>
    internal static class WPFRegistryPaths
    {
        /// <summary>
        /// The base key path
        /// </summary>
        public static string BaseKeyPath => RCFRegistryPaths.GetPath("WPF");
    }
}