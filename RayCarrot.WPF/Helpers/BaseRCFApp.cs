using Microsoft.Extensions.Logging;
using RayCarrot.CarrotFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.Extensions;
using RayCarrot.IO;

namespace RayCarrot.WPF
{
    /// <summary>
    /// A base <see cref="Application"/> to inherit from for a Carrot Framework WPF application
    /// </summary>
    public abstract class BaseRCFApp : Application
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="useMutex">Indicates if a <see cref="Mutex"/> should be used to only allow a single instance of the application.
        /// This requires a valid GUID in the entry assembly.</param>
        /// <param name="splashScreenResourceName">The resource name for a splash screen if one is to be used</param>
        protected BaseRCFApp(bool useMutex, string splashScreenResourceName = null)
        {
            // Create the startup timer and start it
            AppStartupTimer = new Stopwatch();
            AppStartupTimer.Start();

            // Create and show the splash screen if one is to be used
            if (splashScreenResourceName != null)
            {
                SplashScreenFadeout = TimeSpan.MinValue;
                SplashScreen = new SplashScreen(splashScreenResourceName);
                SplashScreen.Show(false);
            }

            StartupTimeLogs = new List<string>();

            // Subscribe to events
            Startup += BaseRCFApp_Startup;
            DispatcherUnhandledException += BaseRCFApp_DispatcherUnhandledException;
            Exit += BaseRCFApp_Exit;

            if (useMutex)
            {
                try
                {
                    var entry = Assembly.GetEntryAssembly();

                    if (entry == null)
                        throw new InvalidOperationException("The application can not use a Mutex for forcing a single instance if no valid entry assembly is found");

                    // Use mutex to only allow one instance of the application at a time
                    Mutex = new Mutex(false, "Global\\" + ((GuidAttribute)entry.GetCustomAttributes(typeof(GuidAttribute), false).GetValue(0)).Value);
                }
                catch (IndexOutOfRangeException ex)
                {
                    throw new InvalidOperationException("The application can not use a Mutex for forcing a single instance if the entry assembly does not have a valid GUID identifier", ex);
                }
            }

            LogStartupTime("Construction finished");
        }

        #endregion

        #region Private Properties

        /// <summary>
        /// The splash screen, if one is used
        /// </summary>
        private SplashScreen SplashScreen { get; }

        /// <summary>
        /// The mutex
        /// </summary>
        private Mutex Mutex { get; }

        /// <summary>
        /// Indicates if the main window is currently closing
        /// </summary>
        private bool IsClosing { get; set; }

        /// <summary>
        /// Indicates if the main window is done closing and can be closed
        /// </summary>
        private bool DoneClosing { get; set; }

        /// <summary>
        /// The timer for the application startup
        /// </summary>
        private Stopwatch AppStartupTimer { get; }

        /// <summary>
        /// The startup time logs to log once the app has started to improve performance
        /// </summary>
        private List<string> StartupTimeLogs { get; }

        #endregion

        #region Protected Properties

        /// <summary>
        /// The fadeout for the splash screen
        /// </summary>
        protected TimeSpan SplashScreenFadeout { get; set; }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the <see cref="Application"/> object for the current <see cref="AppDomain"/> as a <see cref="BaseRCFApp"/>.
        /// </summary>
        public new static BaseRCFApp Current => Application.Current as BaseRCFApp;

        /// <summary>
        /// Gets the active <see cref="Window"/>
        /// </summary>
        public Window CurrentActiveWindow => Windows.OfType<Window>().FindItem(x => x.IsActive);

        #endregion

        #region Private Methods

        /// <summary>
        /// Handles the application startup
        /// </summary>
        /// <param name="args">The launch arguments</param>
        private async void AppStartupAsync(string[] args)
        {
            LogStartupTime("App startup begins");

            // Set the shutdown mode to avoid any license windows to close the application
            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            // Make sure the application can launch
            if (!await InitialSetupAsync(args))
            {
                Shutdown();
                return;
            }

            LogStartupTime("Initial setup has been verified");

            // Set up the framework
            SetupFramework(args);

            LogStartupTime("Framework has been setup");

            // Log the current environment
            try
            {
                RCFCore.Logger?.LogInformationSource($"Current platform: {Environment.OSVersion.VersionString}");

            }
            catch (Exception ex)
            {
                ex.HandleError("Logging environment details");
            }

            // Log some debug information
            RCFCore.Logger?.LogDebugSource($"Entry assembly path: {Assembly.GetEntryAssembly()?.Location}");

            LogStartupTime("Debug info has been logged");

            // Run startup
            await OnSetupAsync(args);

            LogStartupTime("Startup has run");

            // Get the main window
            var mainWindow = GetMainWindow();

            LogStartupTime("Main window has been created");

            // Subscribe to events
            mainWindow.Loaded += MainWindow_Loaded;
            mainWindow.Closing += MainWindow_ClosingAsync;
            mainWindow.Closed += MainWindow_Closed;

            // Close splash screen
            CloseSplashScreen();

            // Show the main window
            mainWindow.Show();

            // Set the shutdown mode
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
        }

        /// <summary>
        /// Sets up the framework
        /// </summary>
        private void SetupFramework(string[] args)
        {
            LogLevel logLevel = LogLevel.Information;

            // Get the log level from launch arguments
            if (args.Contains("-loglevel"))
            {
                try
                {
                    string ll = args[args.FindItemIndex(x => x == "-loglevel") + 1];
                    logLevel = Enum.Parse(typeof(LogLevel), ll, true).CastTo<LogLevel>();
                }
                catch (Exception ex)
                {
                    ex.HandleError("Setting user level from args");
                }
            }

            // Create the construction
            var construction = new FrameworkConstruction();

            // Set up the framework
            var config = SetupFramework(construction, logLevel, args);

            // Build the framework
            construction.Build(config);

            RCFCore.Logger?.LogInformationSource($"The log level has been set to {logLevel}");

            // Retrieve arguments
            RCFCore.Data.Arguments = args;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Logs the startup time
        /// </summary>
        /// <param name="logDescription">The log description</param>
        protected void LogStartupTime(string logDescription)
        {
            StartupTimeLogs.Add($"Startup: {AppStartupTimer.ElapsedMilliseconds} ms - {logDescription}");
        }

        /// <summary>
        /// Closes the splash screen if showing
        /// </summary>
        protected void CloseSplashScreen()
        {
            // Close splash screen
            SplashScreen?.Close(SplashScreenFadeout);
        }

        #endregion

        #region Event Handlers

        private void BaseRCFApp_Startup(object sender, StartupEventArgs e)
        {
            LogStartupTime("Startup event called");

            try
            {
                if (Mutex != null && !Mutex.WaitOne(0, false))
                {
                    OnOtherInstanceFound(e.Args);
                    Shutdown();
                    return;
                }
            }
#pragma warning disable CS0168 // Variable is declared but never used
            catch (AbandonedMutexException ex)
#pragma warning restore CS0168 // Variable is declared but never used
            {
                // Break if debugging
                Debugger.Break();
            }

            AppStartupAsync(e.Args);
        }

        private void BaseRCFApp_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                if (RCF.IsBuilt)
                {
                    // Handle the exception
                    e.Exception.HandleCritical("Unhandled exception");

                    RCFCore.Logger?.LogCriticalSource("An unhandled exception has occurred");
                }

                // Get the path to log to
                FileSystemPath logPath = Path.Combine(Directory.GetCurrentDirectory(), "crashlog.txt");

                // Write log
                File.WriteAllLines(logPath, SessionLogger.Logs?.Select(x => $"[{x.LogLevel}] {x.Message}") ?? new string[] { "Service not available" });

                // Notify user
                MessageBox.Show($"The application crashed with the following exception message:{Environment.NewLine}{e.Exception.Message}{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}" +
                                $"A crash log has been created under {logPath}.", "Critical error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception)
            {
                // Notify user
                MessageBox.Show($"The application crashed with the following exception message:{Environment.NewLine}{e?.Exception?.Message}{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}", "Critical error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // Close splash screen
                CloseSplashScreen();

                // Dispose mutex
                Mutex?.Dispose();
            }
        }

        private void BaseRCFApp_Exit(object sender, ExitEventArgs e)
        {
            // Close splash screen
            CloseSplashScreen();

            // Dispose mutex
            Mutex?.Dispose();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Add startup time log
            LogStartupTime("Main window loaded");

            // Stop the stopwatch
            AppStartupTimer.Stop();

            // Log all startup time logs
            foreach (string log in StartupTimeLogs)
                RCFCore.Logger?.LogDebugSource(log);

            // Clear the startup time logs
            StartupTimeLogs.Clear();
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            // Shutdown the application
            Shutdown();
        }

        private async void MainWindow_ClosingAsync(object sender, CancelEventArgs e)
        {
            if (DoneClosing)
                return;

            e.Cancel = true;

            if (IsClosing)
                return;

            IsClosing = true;

            RCFCore.Logger?.LogInformationSource("The main window is closing...");

            try
            {
                // Get the main window
                var mainWindow = sender as Window ?? MainWindow;

                // Attempt to close all other windows
                foreach (Window window in Windows)
                {
                    if (window == mainWindow)
                        continue;

                    window.Focus();
                    window.Close();
                }

                // Make sure all other windows have been closed
                if (Windows.Count > 1)
                {
                    RCFCore.Logger?.LogInformationSource("The shutdown was cancelled due to one or more windows still being open");

                    IsClosing = false;
                    return;
                }

                await OnCloseAsync(mainWindow);

                DoneClosing = true;

                // Close application
                Shutdown();
            }
            catch (Exception ex)
            {
                // Attempt to log the exception, ignoring any exceptions thrown
                new Action(() => ex.HandleError("Closing main window")).IgnoreIfException();

                // Notify the user of the error
                MessageBox.Show($"An error occured when shutting down the application. Error message: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                DoneClosing = true;

                // Close application
                Shutdown();
            }
        }

        #endregion

        #region Protected Abstract Methods

        /// <summary>
        /// Gets the main <see cref="Window"/> to show
        /// </summary>
        /// <returns>The Window instance</returns>
        protected abstract Window GetMainWindow();

        /// <summary>
        /// Sets up the framework with loggers and other services
        /// </summary>
        /// <param name="construction">The construction</param>
        /// <param name="logLevel">The level to log</param>
        /// <param name="args">The launch arguments</param>
        /// <returns>The configuration values to pass on to the framework, if any</returns>
        protected abstract IDictionary<string, object> SetupFramework(IFrameworkConstruction construction, LogLevel logLevel, string[] args);

        #endregion

        #region Protected Virtual Methods

        /// <summary>
        /// An optional custom setup to override
        /// </summary>
        /// <param name="args">The launch arguments</param>
        /// <returns>The task</returns>
        protected virtual Task OnSetupAsync(string[] args)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// An optional method to override which runs when closing
        /// </summary>
        /// <param name="mainWindow">The main Window of the application</param>
        /// <returns>The task</returns>
        protected virtual Task OnCloseAsync(Window mainWindow)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Override to run when another instance of the program is found running
        /// </summary>
        /// <param name="args">The launch arguments</param>
        protected virtual void OnOtherInstanceFound(string[] args)
        {

        }

        /// <summary>
        /// Optional initial setup to run. Can be used to check if the environment is valid
        /// for the application to run or for the user to accept the license.
        /// </summary>
        /// <param name="args">The launch arguments</param>
        /// <returns>True if the setup finished successfully or false if the application has to shut down</returns>
        protected virtual Task<bool> InitialSetupAsync(string[] args)
        {
            return Task.FromResult(true);
        }

        #endregion
    }
}
