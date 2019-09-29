using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.UI;
using RayCarrot.Windows.Registry;
using RayCarrot.WPF;

[assembly: RCFDefaultService(typeof(IDialogBaseManager), typeof(WindowDialogBaseManager), true)]
[assembly: RCFDefaultService(typeof(IMessageUIManager), typeof(DefaultWPFMessageUIManager), true)]
[assembly: RCFDefaultService(typeof(IBrowseUIManager), typeof(DefaultWPFBrowseUIManager), true)]
[assembly: RCFDefaultService(typeof(IRegistryBrowseUIManager), typeof(DefaultWPFRegistryBrowseUIManager), true)]

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
        public static IDialogBaseManager DialogBaseManager => RCF.GetService<IDialogBaseManager>(false);
    }
}