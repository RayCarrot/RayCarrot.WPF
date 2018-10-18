using RayCarrot.CarrotFramework;
using RayCarrot.CarrotFramework.UI;
using System.ComponentModel;
using System.Windows;

namespace RayCarrot.WPF
{
    /// <summary>
    /// A <see cref="Window"/> with support for common dialog options
    /// </summary>
    /// <typeparam name="VM">The view model</typeparam>
    /// <typeparam name="Result">The dialog result</typeparam>
    public abstract class DialogVMWindow<VM, Result> : VMWindow<VM>
        where VM : class, INotifyPropertyChanged, new()
        where Result : UserInputResult
    {
        #region Constructors

        public DialogVMWindow() : this(new VM())
        {

        }

        public DialogVMWindow(VM viewModel) : base(viewModel)
        {
            // Attempt to get default Window style from Framework
            Style = RCF.GetService<IWPFStyle>(false)?.WindowStyle ?? Style;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Shows a dialog of this instance and returns the result
        /// </summary>
        /// <returns>The result</returns>
        public new Result ShowDialog()
        {
            base.ShowDialog();
            return GetResult();
        }

        #endregion

        #region Abstract Methods

        /// <summary>
        /// Gets the current result for the dialog
        /// </summary>
        /// <returns>The result</returns>
        protected abstract Result GetResult();

        #endregion
    }
}