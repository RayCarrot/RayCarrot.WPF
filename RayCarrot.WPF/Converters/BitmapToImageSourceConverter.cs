using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace RayCarrot.WPF
{
    /// <summary>
    /// Converts a <see cref="Bitmap"/> to a <see cref="ImageSource"/>
    /// </summary>
    public class BitmapToImageSourceConverter : BaseValueConverter<BitmapToImageSourceConverter, Bitmap, ImageSource>
    {
        public override ImageSource ConvertValue(Bitmap value, Type targetType, object parameter, CultureInfo culture)
        {
            //using (var stream = new MemoryStream())
            //{
            //    value.Save(stream, ImageFormat.Png);
            //    return BitmapFrame.Create(stream);
            //}

            var rect = new Rectangle(0, 0, value.Width, value.Height);

            BitmapData bitmapData = value.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            try
            {
                var size = (rect.Width * rect.Height) * 4;

                return BitmapSource.Create(value.Width, value.Height, value.HorizontalResolution, value.VerticalResolution, PixelFormats.Bgra32, null, bitmapData.Scan0, size, bitmapData.Stride);
            }
            finally
            {
                value.UnlockBits(bitmapData);
            }
        }
    }
}