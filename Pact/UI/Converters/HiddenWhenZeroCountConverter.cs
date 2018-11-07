using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Pact
{
    public sealed class HiddenWhenZeroCountConverter
        : IValueConverter
    {
        object IValueConverter.Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            if (value is int count && count == 0)
                return Visibility.Hidden;

            return Visibility.Visible;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
