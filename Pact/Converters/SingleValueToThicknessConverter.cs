using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Pact
{
    public sealed class SingleValueToThicknessConverter
        : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var result = new Thickness();

            if (value is int intValue)
            {
                if (string.Equals(parameter?.ToString(), "top", StringComparison.OrdinalIgnoreCase))
                    result.Top = intValue;
                else if (string.Equals(parameter?.ToString(), "right", StringComparison.OrdinalIgnoreCase))
                    result.Right = intValue;
                else if (string.Equals(parameter?.ToString(), "bottom", StringComparison.OrdinalIgnoreCase))
                    result.Bottom = intValue;
                else
                    result.Left = intValue;
            }

            return result;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
