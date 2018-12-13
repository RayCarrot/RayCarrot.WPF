using System;
using System.Globalization;
using RayCarrot.CarrotFramework;

namespace RayCarrot.WPF
{
    /// <summary>
    /// Converts a <see cref="String"/> to a <see cref="FileSystemPath"/>
    /// </summary>
    public class StringToFileSystemPathConverter : BaseValueConverter<StringToFileSystemPathConverter, string, FileSystemPath>
    {
        public override FileSystemPath ConvertValue(string value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public override string ConvertValueBack(FileSystemPath value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}