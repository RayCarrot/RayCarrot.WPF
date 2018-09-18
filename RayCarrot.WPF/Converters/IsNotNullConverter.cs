using System;
using System.Globalization;

namespace RayCarrot.WPF
{
    /// <summary>
    /// Converts a <see cref="Object"/> to a <see cref="Boolean"/> which is true when the value is not null
    /// </summary>
    public class IsNotNullConverter : BaseValueConverter<IsNotNullConverter, object, bool>
    {
        public override bool ConvertValue(object value, Type targetType, object parameter, CultureInfo culture) => !(value is null);
    }
}