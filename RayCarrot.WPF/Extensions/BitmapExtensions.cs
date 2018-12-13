using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace RayCarrot.WPF
{
    /// <summary>
    /// Extension methods for <see cref="Bitmap"/>
    /// </summary>
    public static class BitmapExtensions
    {
        /// <summary>
        /// Converts a <see cref="Bitmap"/> to an <see cref="ImageSource"/>
        /// </summary>
        /// <param name="bmp"></param>
        /// <returns></returns>
        public static ImageSource ToImageSource(this Bitmap bmp)
        {
            //using (var stream = new MemoryStream())
            //{
            //    value.Save(stream, ImageFormat.Png);
            //    return BitmapFrame.Create(stream);
            //}

            var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);

            BitmapData bitmapData = bmp.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            try
            {
                var size = (rect.Width * rect.Height) * 4;

                return BitmapSource.Create(bmp.Width, bmp.Height, bmp.HorizontalResolution, bmp.VerticalResolution, PixelFormats.Bgra32, null, bitmapData.Scan0, size, bitmapData.Stride);
            }
            finally
            {
                bmp.UnlockBits(bitmapData);
            }
        }
    }
}