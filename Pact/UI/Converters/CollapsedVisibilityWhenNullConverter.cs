using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Pact
{
    public sealed class CollapsedVisibilityWhenNullConverter
        : IValueConverter
    {
        object IValueConverter.Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            bool expectingNullValue = true;
            if (bool.TryParse(parameter?.ToString(), out bool boolParameter))
                expectingNullValue = boolParameter;

            return (value == null) == expectingNullValue ? Visibility.Collapsed : Visibility.Visible;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
