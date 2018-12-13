using System.Windows;
using System.Windows.Media;

namespace RayCarrot.WPF
{
    /// <summary>
    /// Extension methods for <see cref="Visual"/>
    /// </summary>
    public static class VisualExtensions
    {
        /// <summary>
        /// Gets the first descendant element by type
        /// </summary>
        /// <typeparam name="T">The type of element to get</typeparam>
        /// <param name="element">The element to get the type from</param>
        /// <returns>The element</returns>
        public static T GetDescendantByType<T>(this Visual element) 
            where T : class
        {
            if (element == null)
                return default;

            if (element.GetType() == typeof(T))
                return element as T;

            if (element is FrameworkElement frameworkElement)
                frameworkElement.ApplyTemplate();

            T foundElement = null;

            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                var visual = VisualTreeHelper.GetChild(element, i) as Visual;
                foundElement = visual.GetDescendantByType<T>();
                if (foundElement != null)
                    break;
            }
            return foundElement;
        }
    }
}