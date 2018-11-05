using RayCarrot.CarrotFramework;
using RayCarrot.CarrotFramework.UI;
using System;
using System.Windows;

namespace RayCarrot.WPF
{
    /// <summary>
    /// Default dialog base manager, showing the dialog in a <see cref="Window"/>
    /// </summary>
    public class WindowDialogBaseManager : IDialogBaseManager
    {
        /// <summary>
        /// Shows the dialog and returns when the dialog closes with a result
        /// </summary>
        /// <typeparam name="V">The view model type</typeparam>
        /// <typeparam name="R">The return type</typeparam>
        /// <param name="dialog">The dialog to show</param>
        /// <param name="owner">The owner window</param>
        /// <returns>The result</returns>
        public R ShowDialog<V, R>(IDialogBaseControl<V, R> dialog, Window owner)
            where V : UserInputViewModel
        {
            // Create the window
            var window = new Window()
            {
                Content = dialog.DialogContent,
                ResizeMode = dialog.Resizable ? ResizeMode.CanResize : ResizeMode.NoResize,
                Title = dialog.ViewModel.Title,
                SizeToContent = dialog.Resizable ? SizeToContent.Manual : SizeToContent.WidthAndHeight
            };

            // Set size if resizable
            if (dialog.Resizable)
            {
                window.Height = 475;
                window.Width = 750;
                window.MinHeight = 300;
                window.MinWidth = 400;
            }

            if (owner != null)
            {
                // Set startup location
                window.WindowStartupLocation = WindowStartupLocation.CenterOwner;

                // Set owner
                window.Owner = owner;
            }

            // Attempt to get default Window style from Framework
            window.Style = RCF.GetService<IWPFStyle>(false)?.WindowStyle ?? window.Style;

            void Dialog_CloseDialog(object sender, EventArgs e)
            {
                window.Close();
            }

            // Close window on request
            dialog.CloseDialog += Dialog_CloseDialog;

            // Show window as dialog
            window.ShowDialog();

            // Unsubscribe
            dialog.CloseDialog -= Dialog_CloseDialog;

            // Return the result
            return dialog.GetResult();
        }
    }
}