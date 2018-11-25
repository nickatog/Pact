using System;
using System.Globalization;
using System.Windows.Data;

namespace Pact.Converters
{
    public sealed class CardCountToOpacity
        : IValueConverter
    {
        object IValueConverter.Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            if (value is int count)
                return count == 0 ? 0.5 : 0;

            return 0;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
