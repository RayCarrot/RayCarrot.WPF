using RayCarrot.CarrotFramework;
using RayCarrot.CarrotFramework.UI;
using System.Collections.Generic;
using System.Linq;

namespace RayCarrot.WPF
{
    /// <summary>
    /// View model for a dialog message
    /// </summary>
    public class DialogMessageViewModel : BaseViewModel
    {
        /// <summary>
        /// The message text
        /// </summary>
        public string MessageText { get; set; }

        /// <summary>
        /// The message header
        /// </summary>
        public string MessageHeader { get; set; }

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

    /// <summary>
    /// Generic view model for a dialog message
    /// </summary>
    public class DialogMessageViewModel<T> : DialogMessageViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public DialogMessageViewModel()
        {
            // Default the default action result
            DefaultActionResult = default;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The dialog actions
        /// </summary>
        public new IList<DialogMessageActionViewModel<T>> DialogActions
        {
            get => base.DialogActions.Cast<DialogMessageActionViewModel<T>>().ToList();
            set => base.DialogActions = value.Cast<DialogMessageActionViewModel>().ToList();
        }

        /// <summary>
        /// The default action result
        /// </summary>
        public new T DefaultActionResult
        {
            get => base.DefaultActionResult.CastTo<T>();
            set => base.DefaultActionResult = value;
        }

        #endregion
    }
}