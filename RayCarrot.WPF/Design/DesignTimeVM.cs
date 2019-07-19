using System.Collections.Generic;
using RayCarrot.UI;

// ReSharper disable StringLiteralTypo

namespace RayCarrot.WPF
{
    /// <summary>
    /// Static view models to bind to during design time
    /// </summary>
    public static class DesignTimeVM
    {
        public static DialogMessageViewModel DialogMessageViewModel => new DialogMessageViewModel()
        {
            MessageText = "This is a text message which will appear for the user to read before pressing a button. Great huh?",
            Title = "This is the header",
            MessageType = MessageType.Error,
            DialogActions = new List<DialogMessageActionViewModel>
            {
                new DialogMessageActionViewModel()
                {
                    DisplayText = "CANCEL",
                    DisplayDescription = "Press the other button. Not this one. Stay AWAY",
                    ActionResult = false
                },
                new DialogMessageActionViewModel()
                {
                    DisplayText = "OK",
                    DisplayDescription = "It's okay. Press the button. I promise",
                    ActionResult = true
                },
            }
        };

        public static DriveSelectionViewModel DriveSelectionViewModel
        {
            get
            {
                var vm = new DriveSelectionViewModel(new DriveBrowserViewModel()
                {
                    Title = "Select a Drive"
                });
                _ = vm.RefreshAsync();
                return vm;
            }
        }

        public static RegistrySelectionViewModel RegistrySelectionViewModel => new RegistrySelectionViewModel();
    }
}