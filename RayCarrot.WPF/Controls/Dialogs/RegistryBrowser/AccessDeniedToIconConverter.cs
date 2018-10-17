using System;
using System.Globalization;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RayCarrot.WPF
{
    /// <summary>
    /// Converts a <see cref="Boolean"/> representing if access is denied to a <see cref="ImageSource"/> for a Registry selection icon
    /// </summary>
    internal class AccessDeniedToIconConverter : BaseValueConverter<AccessDeniedToIconConverter, bool, ImageSource>
    {
        public override ImageSource ConvertValue(bool value, Type targetType, object parameter, CultureInfo culture)
        {
            var path = "pack://application:,,,/RayCarrot.WPF;component/Controls/Dialogs/RegistryBrowser/Icons/";

            return new BitmapImage(new Uri(value ? path + "Hidden Folder.png" : path + "Folder.png"));
        }
    }
}