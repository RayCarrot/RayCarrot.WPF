using RayCarrot.CarrotFramework.UI;
using System.ComponentModel;
using System.Windows;

namespace RayCarrot.WPF
{
    /// <summary>
    /// A <see cref="Window"/> with view model support
    /// </summary>
    /// <typeparam name="VM">The type of view model to use</typeparam>
    public class VMWindow<VM> : Window
        where VM : class, INotifyPropertyChanged, new()
    {
        #region Constructors

        public VMWindow()
        {
            ViewModel = new VM();
        }

        public VMWindow(VM viewModel)
        {
            ViewModel = viewModel;
        }

        #endregion

        #region Protected Properties

        protected VM ViewModel
        {
            get => DataContext as VM;
            set => DataContext = value;
        }

        #endregion
    }
}