using System;
using System.Globalization;
using System.Windows.Data;

namespace Pact
{
    public sealed class HiddenVisibilityWhenMatchesBoolConverter
        : IValueConverter
    {
        object IValueConverter.Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            if (value is bool boolValue
                && bool.TryParse(parameter?.ToString(), out bool boolParameter)
                && boolValue == boolParameter)
                return -1;

            return 1;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
