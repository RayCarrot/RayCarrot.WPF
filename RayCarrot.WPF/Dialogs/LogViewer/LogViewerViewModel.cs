using System;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.Extensions;
using RayCarrot.UI;

namespace RayCarrot.WPF
{
    /// <summary>
    /// View model for a log viewer
    /// </summary>
    public class LogViewerViewModel : UserInputViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public LogViewerViewModel()
        {
            // Set a default title
            Title = "Log Viewer";

            // Create commands
            RefreshDisplayLogCommand = new RelayCommand(RefreshDisplayLog);
            ClearLogCommand = new RelayCommand(ClearLog);

            // Refresh the logs
            RefreshDisplayLog();

            // Subscribe to when new logs get added
            RCFCore.Logs.LogAdded += (s, e) => RefreshDisplayLog();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Refreshes the display log
        /// </summary>
        public void RefreshDisplayLog()
        {
            lock (this)
            {
                try
                {
                    DisplayLog = RCFCore.Logs?.GetLogs().Where(x => x.LogLevel >= ShowLogLevel).ToObservableCollection();
                }
                catch (Exception ex)
                {
                    ex.HandleUnexpected("Refreshing log viewer");
                }
            }
        }

        /// <summary>
        /// Clears the log
        /// </summary>
        public void ClearLog()
        {
            lock (this)
            {
                RCFCore.Logs?.Clear();
                DisplayLog.Clear();
            }
        }

        #endregion

        #region Commands

        public ICommand RefreshDisplayLogCommand { get; }

        public ICommand ClearLogCommand { get; }

        #endregion

        #region Public Properties

        /// <summary>
        /// The logs to be displayed
        /// </summary>
        public ObservableCollection<LogItem> DisplayLog { get; set; }

        /// <summary>
        /// The log level to show
        /// </summary>
        public LogLevel ShowLogLevel
        {
            get => _showLogLevel;
            set
            {
                if (value == _showLogLevel)
                    return;

                _showLogLevel = value;
                RefreshDisplayLog();
            }
        }

        #endregion

        #region Private Properties

        private LogLevel _showLogLevel = LogLevel.Trace;

        #endregion
    }
}