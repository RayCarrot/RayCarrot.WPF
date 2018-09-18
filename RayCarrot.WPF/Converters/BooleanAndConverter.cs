﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace RayCarrot.WPF
{
    /// <summary>
    /// Converts an array of <see cref="Boolean"/> to a <see cref="Boolean"/> determining if all values are true
    /// </summary>
    public class BooleanAndConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            foreach (object value in values)
            {
                if ((value is bool) && (bool)value == false)
                {
                    return false;
                }
            }
            return true;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("BooleanAndConverter is a OneWay converter.");
        }
    }
}