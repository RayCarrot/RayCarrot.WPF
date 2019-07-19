using System;
using RayCarrot.Extensions;

namespace RayCarrot.WPF
{
    /// <summary>
    /// Event arguments for when a dialog message action is handled
    /// </summary>
    public class DialogMessageActionHandledEventArgs : EventArgs
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="actionResult">The action result</param>
        public DialogMessageActionHandledEventArgs(object actionResult)
        {
            ActionResult = actionResult;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The action result
        /// </summary>
        public object ActionResult { get; }

        #endregion
    }

    /// <summary>
    /// Event arguments for when a dialog message action is handled
    /// </summary>
    public class DialogMessageActionHandledEventArgs<T> : DialogMessageActionHandledEventArgs
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="actionResult">The action result</param>
        public DialogMessageActionHandledEventArgs(T actionResult) : base(actionResult)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// The action result
        /// </summary>
        public new T ActionResult => base.ActionResult.CastTo<T>();

        #endregion
    }
}