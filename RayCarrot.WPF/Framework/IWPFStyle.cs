using System.Windows;

namespace RayCarrot.WPF
{
    /// <summary>
    /// Contains reusable styles for a WPF application
    /// </summary>
    public interface IWPFStyle
    {
        /// <summary>
        /// The <see cref="Window"/> style
        /// </summary>
        Style WindowStyle { get; }
    }
}