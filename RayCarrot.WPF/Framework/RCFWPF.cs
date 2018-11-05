using RayCarrot.CarrotFramework;

namespace RayCarrot.WPF
{
    /// <summary>
    /// Shortcuts to the Carrot Framework
    /// </summary>
    public static class RCFWPF
    {
        /// <summary>
        /// Gets the WPF style
        /// </summary>
        public static IWPFStyle WPFStyle => RCF.GetService<IWPFStyle>();

        /// <summary>
        /// Gets the dialog base manager, or the default one
        /// </summary>
        public static IDialogBaseManager DialogBaseManager => RCF.GetService<IDialogBaseManager>(false) ?? new WindowDialogBaseManager();
    }
}