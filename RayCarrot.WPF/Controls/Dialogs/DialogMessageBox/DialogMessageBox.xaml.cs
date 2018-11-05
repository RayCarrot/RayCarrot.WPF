﻿using RayCarrot.CarrotFramework;
using System;
using System.Windows;
using System.Windows.Controls;

namespace RayCarrot.WPF
{
    /// <summary>
    /// A dialog message box with standard WPF controls
    /// </summary>
    public partial class DialogMessageBox : UserControl, IDialogBaseControl<DialogMessageViewModel, object>
    {
        #region Constructor

        /// <summary>
        /// Creates a new instance of <see cref="DialogMessageBox"/>
        /// </summary>
        /// <param name="dialogVM">The dialog view model</param>
        public DialogMessageBox(DialogMessageViewModel dialogVM) : this(dialogVM, null)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="DialogMessageBox"/>
        /// </summary>
        /// <param name="dialogVM">The dialog view model</param>
        /// <param name="owner">The owner window</param>
        public DialogMessageBox(DialogMessageViewModel dialogVM, Window owner)
        {
            InitializeComponent();

            // Set the data context
            DataContext = dialogVM;

            // Reset the result
            DialogResult = ViewModel.DefaultActionResult;

            // Subscribe to events
            ViewModel.DialogActions?.ForEach(x => x.ActionHandled += DialogAction_ActionHandled);
        }

        #endregion

        #region Protected Properties

        /// <summary>
        /// The dialog result
        /// </summary>
        protected object DialogResult { get; set; }

        #endregion

        #region Public Properties

        /// <summary>
        /// The view model
        /// </summary>
        public DialogMessageViewModel ViewModel => DataContext as DialogMessageViewModel;

        /// <summary>
        /// The dialog content
        /// </summary>
        public object DialogContent => this;

        /// <summary>
        /// Indicates if the dialog should be resizable
        /// </summary>
        public bool Resizable => false;

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the current result
        /// </summary>
        /// <returns>The result</returns>
        public object GetResult() => DialogResult;

        #endregion

        #region Events

        /// <summary>
        /// Invoke to request the dialog to close
        /// </summary>
        public event EventHandler CloseDialog;

        #endregion

        #region Event Handler

        private void DialogAction_ActionHandled(object sender, DialogMessageActionHandledEventArgs e)
        {
            DialogResult = e.ActionResult;
            CloseDialog?.Invoke(this, new EventArgs());
        }

        #endregion
    }
}