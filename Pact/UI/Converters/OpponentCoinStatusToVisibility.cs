using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Pact.Converters
{
    public sealed class OpponentCoinStatusToVisibility
        : IValueConverter
    {
        object IValueConverter.Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            var parameterValue = parameter as string;

            bool show = parameterValue == "1";

            if (value is bool opponentCoinStatus)
                show = opponentCoinStatus ? parameterValue == "2" : parameterValue == "3";

            return show ? Visibility.Visible : Visibility.Collapsed;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
