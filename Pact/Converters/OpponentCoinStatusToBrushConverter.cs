using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Pact
{
    public class OpponentCoinStatusToBrushConverter
        : IValueConverter
    {
        object IValueConverter.Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            if (value is bool opponentCoinStatus)
                return opponentCoinStatus ? Brushes.Yellow : Brushes.Red;

            return Brushes.Green;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
