using RayCarrot.CarrotFramework;
using RayCarrot.Windows;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace RayCarrot.WPF
{
    /// <summary>
    /// The default <see cref="IRegistryBrowseUIManager"/> for Windows Forms
    /// </summary>
    public class DefaultWPFRegistryBrowseUIManager : IRegistryBrowseUIManager
    {
        /// <summary>
        /// Indicates if the browse requests should be logged
        /// </summary>
        public virtual bool LogRequests { get; set; }

        /// <summary>
        /// Allows to user to browse for a Registry key and optionally a value
        /// </summary>
        /// <param name="registryBrowserViewModel">The Registry browser information</param>
        /// <returns>The Registry browser result</returns>
        public async Task<RegistryBrowserResult> BrowseRegistryKeyAsync(RegistryBrowserViewModel registryBrowserViewModel, [CallerMemberName] string origin = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            if (LogRequests)
                RCF.Logger.LogTraceSource($"A browse Registry key dialog was opened with the title of: {registryBrowserViewModel.Title}", origin: origin, filePath: filePath, lineNumber: lineNumber);

            return await new RegistrySelectionDialog(registryBrowserViewModel).ShowDialogAsync();
        }
    }
}