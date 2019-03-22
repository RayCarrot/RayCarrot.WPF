﻿using Microsoft.Extensions.Logging;
using RayCarrot.CarrotFramework;
using System;
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
        protected BaseRCFApp(bool useMutex)
        {
            Startup += BaseRCFApp_Startup;
            DispatcherUnhandledException += BaseRCFApp_DispatcherUnhandledException;
            Exit += BaseRCFApp_Exit;

            if (useMutex)
            {
                try
                {
                    // Use mutex to only allow one instance of the application at a time
                    Mutex = new Mutex(false, "Global\\" + ((GuidAttribute)Assembly.GetEntryAssembly().GetCustomAttributes(typeof(GuidAttribute), false).GetValue(0)).Value);
                }
                catch (IndexOutOfRangeException ex)
                {
                    throw new InvalidOperationException("The application can not use a Mutex for forcing a single instance if the entry assembly does not have a valid GUID identifier", ex);
                }
            }
        }

        #endregion

        #region Private Properties

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

        #endregion

        #region Private Methods

        /// <summary>
        /// Handles the application startup
        /// </summary>
        /// <param name="args">The launch arguments</param>
        private async void AppStartupAsync(string[] args)
        {
            // Set the shutdown mode to avoid any license windows to close the application
            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            // Make sure the application can launch
            if (!await InitialSetupAsync(args))
            {
                Shutdown();
                return;
            }

            // Set up the framework
            SetupFramework(args);

            // Log the current environment
            try
            {
                RCF.Logger.LogTraceSource($"Current platform: {Environment.OSVersion.VersionString}");

            }
            catch (Exception ex)
            {
                ex.HandleError("Logging environment details");
            }

            // Log some debug information
            RCF.Logger.LogDebugSource($"Executing assembly path: {Assembly.GetExecutingAssembly().Location}");

            // Run startup
            await OnSetupAsync(args);

            // Get the main window
            var mainWindow = GetMainWindow();

            // Subscribe to events
            mainWindow.Closing += MainWindow_ClosingAsync;
            mainWindow.Closed += MainWindow_Closed;

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
            SetupFramework(construction, logLevel);

            // Build the framework
            construction.Build();

            RCF.Logger.LogInformationSource($"The log level has been set to {logLevel}");

            // Retrieve arguments
            RCF.Data.Arguments = args;
        }

        #endregion

        #region Event Handlers

        private void BaseRCFApp_Startup(object sender, StartupEventArgs e)
        {
            try
            {
                if (!Mutex.WaitOne(0, false))
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

                    RCF.Logger.LogCriticalSource("An unhandled exception has occurred");
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
                MessageBox.Show($"The application crashed with the following exception message:{Environment.NewLine}{e.Exception.Message}{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}", "Critical error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // Dispose mutex
                Mutex?.Dispose();
            }
        }

        private void BaseRCFApp_Exit(object sender, ExitEventArgs e)
        {
            // Dispose mutex
            Mutex?.Dispose();
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

            RCF.Logger.LogInformationSource("The main window is closing...");

            try
            {
                await OnCloseAsync(sender as Window ?? MainWindow);

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
        protected abstract void SetupFramework(FrameworkConstruction construction, LogLevel logLevel);

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