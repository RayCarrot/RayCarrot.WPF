using RayCarrot.CarrotFramework;

namespace RayCarrot.WPF
{
    /// <summary>
    /// Extension methods for <see cref="FrameworkConstruction"/>
    /// </summary>
    public static class FrameworkConstructionExtensions
    {
        /// <summary>
        /// Adds a <see cref="IWPFStyle"/> to the construction
        /// </summary>
        /// <typeparam name="S">The style to add</typeparam>
        /// <param name="construction">The construction</param>
        /// <returns>The construction</returns>
        public static FrameworkConstruction AddWPFStyle<S>(this FrameworkConstruction construction)
            where S : class, IWPFStyle, new ()
        {
            // Add the service
            construction.AddTransient<IWPFStyle, S>();

            // Return the framework
            return construction;
        }
    }
}