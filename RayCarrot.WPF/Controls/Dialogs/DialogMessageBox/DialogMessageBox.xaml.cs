using RayCarrot.CarrotFramework;
using System.Windows;

namespace RayCarrot.WPF
{
    /// <summary>
    /// A dialog message box with standard WPF controls
    /// </summary>
    public partial class DialogMessageBox : Window
    {
        #region Constructor

        /// <summary>
        /// Creates a new instance of <see cref="DialogMessageBox"/>
        /// </summary>
        /// <param name="dialogVM">The dialog view model</param>
        public DialogMessageBox(DialogMessageViewModel dialogVM, Window owner)
        {
            DialogVM = dialogVM;

            if (owner != null)
            {
                Owner = owner;
                WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
            else
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            InitializeComponent();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Shows the dialog and returns the <see cref="DialogResult"/>
        /// </summary>
        /// <returns>The <see cref="DialogResult"/></returns>
        public new object ShowDialog()
        {
            base.ShowDialog();
            return DialogResult;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The dialog result
        /// </summary>
        public new object DialogResult { get; private set; }

        /// <summary>
        /// The dialog view model
        /// </summary>
        public DialogMessageViewModel DialogVM
        {
            get => DataContext as DialogMessageViewModel;
            set
            {
                // Unsubscribe previous events
                DialogVM?.DialogActions?.ForEach(x => x.ActionHandled -= DialogAction_ActionHandled);

                // Set the data context
                DataContext = value;

                // Reset the result
                DialogResult = DialogVM.DefaultActionResult;

                // Subscribe to new events
                DialogVM?.DialogActions?.ForEach(x => x.ActionHandled += DialogAction_ActionHandled);
            }
        }

        #endregion

        #region Event Handler

        private void DialogAction_ActionHandled(object sender, DialogMessageActionHandledEventArgs e)
        {
            DialogResult = e.ActionResult;
            Close();
        }

        #endregion
    }

    /// <summary>
    /// A generic dialog message box with standard WPF controls
    /// </summary>
    public class DialogMessageBox<T> : DialogMessageBox
    {
        #region Constructor

        /// <summary>
        /// Creates a new instance of <see cref="DialogMessageBox"/>
        /// </summary>
        /// <param name="dialogVM">The generic dialog view model</param>
        public DialogMessageBox(DialogMessageViewModel<T> dialogVM, Window owner) : base(dialogVM, owner)
        {
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Shows the dialog and returns the <see cref="DialogResult"/>
        /// </summary>
        /// <returns>The <see cref="DialogResult"/></returns>
        public new T ShowDialog()
        {
            return base.ShowDialog().CastTo<T>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// The dialog result
        /// </summary>
        public new T DialogResult { get; private set; }

        /// <summary>
        /// The dialog view model
        /// </summary>
        public new DialogMessageViewModel<T> DialogVM
        {
            get => base.DialogVM as DialogMessageViewModel<T>;
            set => base.DialogVM = value;
        }

        #endregion
    }
}