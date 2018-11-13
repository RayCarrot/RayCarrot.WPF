using RayCarrot.CarrotFramework.UI;
using System;

namespace RayCarrot.WPF
{
    /// <summary>
    /// Used for dialog controls for <see cref="IDialogBaseControl{V, R}"/>
    /// </summary>
    /// <typeparam name="V">The view model type</typeparam>
    /// <typeparam name="R">The result type</typeparam>
    public interface IDialogBaseControl<V, R>
        where V : UserInputViewModel
    {
        #region Properties

        /// <summary>
        /// The view model
        /// </summary>
        V ViewModel { get; }

        /// <summary>
        /// The dialog content
        /// </summary>
        object DialogContent { get; }

        /// <summary>
        /// Indicates if the dialog should be resizable
        /// </summary>
        bool Resizable { get; }

        /// <summary>
        /// The base size for the dialog
        /// </summary>
        DialogBaseSize BaseSize { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the current result
        /// </summary>
        /// <returns>The result</returns>
        R GetResult();

        #endregion

        #region Events

        /// <summary>
        /// Invoke to request the dialog to close
        /// </summary>
        event EventHandler CloseDialog;

        #endregion
    }
}