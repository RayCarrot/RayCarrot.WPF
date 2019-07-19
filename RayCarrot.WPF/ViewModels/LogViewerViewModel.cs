using System;
using Microsoft.Extensions.Logging;
using RayCarrot.CarrotFramework;
using System.Collections.ObjectModel;
using System.Linq;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.Extensions;
using RayCarrot.UI;

namespace RayCarrot.WPF
{
    /// <summary>
    /// View model for a log viewer
    /// </summary>
    public class LogViewerViewModel : BaseViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public LogViewerViewModel()
        {
            RefreshDisplayLog();
            SessionLogger.LogAdded += (s, e) => RefreshDisplayLog();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Refreshes the display log
        /// </summary>
        public void RefreshDisplayLog()
        {
            lock (this)
            {
                try
                {
                    DisplayLog = SessionLogger.Logs.Where(x => x.LogLevel >= ShowLogLevel).ToObservableCollection();
                }
                catch (Exception ex)
                {
                    ex.HandleUnexpected("Refreshing log viewer");
                }
            }
        }

        #endregion

        #region Commands

        private RelayCommand _RefreshDisplayLogCommand;

        public RelayCommand RefreshDisplayLogCommand => _RefreshDisplayLogCommand ?? (_RefreshDisplayLogCommand = new RelayCommand(RefreshDisplayLog));

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