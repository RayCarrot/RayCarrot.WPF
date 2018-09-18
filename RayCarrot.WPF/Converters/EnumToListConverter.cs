using System;
using System.Globalization;

namespace RayCarrot.WPF
{
    /// <summary>
    /// Converts an <see cref="Enum"/> to a list of strings
    /// </summary>
    public class EnumToListConverter : BaseValueConverter<EnumToListConverter, Enum, object>
    {
        public override object ConvertValue(Enum value, Type targetType, object parameter, CultureInfo culture)
        {
            return Enum.GetNames(value.GetType());
        }
    }
}