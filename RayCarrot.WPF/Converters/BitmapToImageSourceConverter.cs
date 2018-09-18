using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RayCarrot.WPF
{
    /// <summary>
    /// Converts a <see cref="Bitmap"/> to a <see cref="ImageSource"/>
    /// </summary>
    public class BitmapToImageSourceConverter : BaseValueConverter<BitmapToImageSourceConverter, Bitmap, ImageSource>
    {
        public override ImageSource ConvertValue(Bitmap value, Type targetType, object parameter, CultureInfo culture)
        {
            using (var stream = new MemoryStream())
            {
                value.Save(stream, ImageFormat.Png);
                return BitmapFrame.Create(stream);
            }
        }
    }
}