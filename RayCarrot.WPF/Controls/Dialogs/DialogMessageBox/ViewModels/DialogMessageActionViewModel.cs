using RayCarrot.CarrotFramework.UI;
using System;
using System.Windows.Input;

namespace RayCarrot.WPF
{
    /// <summary>
    /// View model for a dialog message action
    /// </summary>
    public class DialogMessageActionViewModel : BaseViewModel
    {
        #region Properties

        /// <summary>
        /// The display text
        /// </summary>
        public string DisplayText { get; set; }

        /// <summary>
        /// The display description
        /// </summary>
        public string DisplayDescription { get; set; }

        /// <summary>
        /// The action result
        /// </summary>
        public object ActionResult { get; set; }

        /// <summary>
        /// True if this is the default action
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// True if this is the default cancel action
        /// </summary>
        public bool IsCancel { get; set; }

        #endregion

        #region Commands

        private ICommand _ActionCommand;

        /// <summary>
        /// Command for when the user selects this action
        /// </summary>
        public ICommand ActionCommand => _ActionCommand ?? (_ActionCommand = new RelayCommand(HandleAction));

        #endregion

        #region Public Methods

        /// <summary>
        /// Handles that the action was chosen by the user
        /// </summary>
        public virtual void HandleAction()
        {
            // Invoke event
            ActionHandled?.Invoke(this, new DialogMessageActionHandledEventArgs(ActionResult));
        }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the action is chosen by the user
        /// </summary>
        public event EventHandler<DialogMessageActionHandledEventArgs> ActionHandled;

        #endregion
    }
}