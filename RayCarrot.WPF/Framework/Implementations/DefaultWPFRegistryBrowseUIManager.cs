using RayCarrot.CarrotFramework;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using RayCarrot.Windows.Registry;

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
        /// <param name="origin">The caller member name (leave at default for compiler-time value)</param>
        /// <param name="filePath">The caller file path (leave at default for compiler-time value)</param>
        /// <param name="lineNumber">The caller line number (leave at default for compiler-time value)</param>
        /// <returns>The Registry browser result</returns>
        public async Task<RegistryBrowserResult> BrowseRegistryKeyAsync(RegistryBrowserViewModel registryBrowserViewModel, [CallerMemberName] string origin = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            if (LogRequests)
                RCF.Logger.LogTraceSource($"A browse Registry key dialog was opened with the title of: {registryBrowserViewModel.Title}", origin: origin, filePath: filePath, lineNumber: lineNumber);

            var result = await new RegistrySelectionDialog(registryBrowserViewModel).ShowDialogAsync();

            RCF.Logger.LogTraceSource(result.CanceledByUser
                ? "The browse Registry key dialog was canceled by the user"
                : $"The browse Registry key dialog returned the selected key path '{result.KeyPath}' and selected value '{result.ValueName}'");

            return result;
        }
    }
}