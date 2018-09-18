using System;
using System.Globalization;
using System.Windows;

namespace RayCarrot.WPF
{
    /// <summary>
    /// Converts a <see cref="String"/> to a <see cref="Visibility"/> which is set to <see cref="Visibility.Visible"/> when the value is not empty
    /// </summary>
    public class StringEmptyToVisibilityConverter : BaseValueConverter<StringEmptyToVisibilityConverter, string, Visibility>
    {
        public override Visibility ConvertValue(string value, Type targetType, object parameter, CultureInfo culture)
        {
            return String.IsNullOrEmpty(value) ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}