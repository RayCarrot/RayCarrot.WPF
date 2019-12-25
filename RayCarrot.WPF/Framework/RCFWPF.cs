using System;
using Microsoft.Extensions.Logging;
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
        #region Shortcuts

        /// <summary>
        /// Gets the WPF style
        /// </summary>
        public static IWPFStyle WPFStyle => RCF.GetService<IWPFStyle>();

        /// <summary>
        /// Gets the dialog base manager, or the default one
        /// </summary>
        public static IDialogBaseManager DialogBaseManager => RCF.GetService<IDialogBaseManager>(false);

        #endregion

        #region Config keys

        /// <summary>
        /// The configuration key for the function which converts a <see cref="LogLevel"/> to a color, represented by a <see cref="String"/>. Value type is <see cref="Func{LogLevel, String}"/>. -1 indicated that all logs will be saved.
        /// </summary>
        public const string LogLevelColorsConfigKey = "MaxLogs";

        #endregion

    }
}