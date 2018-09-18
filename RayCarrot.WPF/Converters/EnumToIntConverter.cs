using System;
using System.Globalization;

namespace RayCarrot.WPF
{
    /// <summary>
    /// Converts a <see cref="Enum"/> to a <see cref="Int32"/> with the type specified in the parameter
    /// </summary>
    public class EnumToIntConverter : BaseValueConverter<EnumToIntConverter, object, int, Type>
    {
        public override int ConvertValue(object value, Type targetType, Type parameter, CultureInfo culture)
        {
            return (int)Enum.Parse(parameter, value.ToString());
        }

        public override object ConvertValueBack(int value, Type targetType, Type parameter, CultureInfo culture)
        {
            return Enum.Parse(parameter, value.ToString());
        }
    }
}