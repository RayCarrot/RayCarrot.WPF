using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using RayCarrot.CarrotFramework;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.Extensions;
using RayCarrot.UI;
using RayCarrot.Windows.Registry;
using RayCarrot.WPF;

namespace RayCarrot.WPF
{
    /// <summary>
    /// Interaction logic for LogViewer.xaml
    /// </summary>
    public partial class LogViewer : UserControl, IDialogBaseControl<LogViewerViewModel, UserInputResult>
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public LogViewer()
        {
            InitializeComponent();

            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            DataContext = new LogViewerViewModel();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the current result for the dialog
        /// </summary>
        /// <returns>The result</returns>
        public UserInputResult GetResult()
        {
            return new UserInputResult()
            {
                CanceledByUser = false
            };
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {

        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The view model
        /// </summary>
        public LogViewerViewModel ViewModel => DataContext as LogViewerViewModel;

        /// <summary>
        /// The dialog content
        /// </summary>
        public object UIContent => this;

        /// <summary>
        /// Indicates if the dialog should be resizable
        /// </summary>
        public bool Resizable => true;

        /// <summary>
        /// The base size for the dialog
        /// </summary>
        public DialogBaseSize BaseSize => DialogBaseSize.Largest;

        #endregion

        #region Event Handlers

        private void LogViewer_Loaded(object sender, RoutedEventArgs e)
        {
            MainScrollViewer.ScrollToBottom();

            // Scroll to bottom when a new log is added
            RCFCore.Logs.LogAdded += (ss, ee) => Dispatcher?.Invoke(() => MainScrollViewer.ScrollToBottom());
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(ViewModel.DisplayLog.Select(x => x.Message).JoinItems(Environment.NewLine));
        }

        #endregion

        #region Events

        /// <summary>
        /// Invoke to request the dialog to close
        /// </summary>
        public event EventHandler CloseDialog;

        #endregion
    }
}