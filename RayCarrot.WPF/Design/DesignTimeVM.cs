using RayCarrot.CarrotFramework.UI;
using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace RayCarrot.WPF
{
    /// <summary>
    /// Static view models to bind to during design time
    /// </summary>
    public static class DesignTimeVM
    {
        public static DialogMessageViewModel DialogMessageViewModel => new DialogMessageViewModel<bool>()
        {
            MessageText = "This is a text message which will appear for the user to read before pressing a button. Great huh?",
            MessageHeader = "This is the header",
            MessageType = MessageType.Error,
            DialogImageSource = @"C:\Users\RayCarrot\Desktop\Allt som jag bryr mig om\Visual Studio\Rayman Game Launcher\Files\Images\Info.png",
            DialogActions = new List<DialogMessageActionViewModel<bool>>
            {
                new DialogMessageActionViewModel<bool>()
                {
                    DisplayText = "CANCEL",
                    DisplayDescription = "Press the other button. Not this one. Stay AWAY",
                    ActionResult = false
                },
                new DialogMessageActionViewModel<bool>()
                {
                    DisplayText = "OK",
                    DisplayDescription = "It's okay. Press the button. I promise",
                    ActionResult = true
                },
            }
        };
    }
}