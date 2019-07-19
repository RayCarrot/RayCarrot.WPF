using RayCarrot.CarrotFramework.Abstractions;

namespace RayCarrot.WPF
{
    /// <summary>
    /// Extension methods for <see cref="IFrameworkConstruction"/>
    /// </summary>
    public static class FrameworkConstructionExtensions
    {
        /// <summary>
        /// Adds a <see cref="IWPFStyle"/> to the construction
        /// </summary>
        /// <typeparam name="S">The style to add</typeparam>
        /// <param name="construction">The construction</param>
        /// <returns>The construction</returns>
        public static IFrameworkConstruction AddWPFStyle<S>(this IFrameworkConstruction construction)
            where S : class, IWPFStyle, new()
        {
            // Add the service
            construction.AddTransient<IWPFStyle, S>();

            // Return the framework
            return construction;
        }

        /// <summary>
        /// Adds a <see cref="IDialogBaseManager"/> to the construction
        /// </summary>
        /// <typeparam name="D">The dialog base manager to add</typeparam>
        /// <param name="construction">The construction</param>
        /// <returns>The construction</returns>
        public static IFrameworkConstruction AddDialogBaseManager<D>(this IFrameworkConstruction construction)
            where D : class, IDialogBaseManager, new()
        {
            // Add the service
            construction.AddTransient<IDialogBaseManager, D>();

            // Return the framework
            return construction;
        }
    }
}