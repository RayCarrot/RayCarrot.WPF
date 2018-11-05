using RayCarrot.CarrotFramework.UI;
using System.Collections.Generic;

namespace RayCarrot.WPF
{
    /// <summary>
    /// View model for a dialog message
    /// </summary>
    public class DialogMessageViewModel : UserInputViewModel
    {
        /// <summary>
        /// The message text
        /// </summary>
        public string MessageText { get; set; }

        /// <summary>
        /// The message type
        /// </summary>
        public MessageType MessageType { get; set; }

        /// <summary>
        /// The dialog actions
        /// </summary>
        public IList<DialogMessageActionViewModel> DialogActions { get; set; }

        /// <summary>
        /// The default action result
        /// </summary>
        public object DefaultActionResult { get; set; }

        /// <summary>
        /// The dialog image source
        /// </summary>
        public string DialogImageSource { get; set; }
    }
}