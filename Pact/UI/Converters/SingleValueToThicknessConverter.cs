using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

using Pact.Extensions.String;

namespace Pact
{
    public sealed class SingleValueToThicknessConverter
        : IValueConverter
    {
        object IValueConverter.Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            var result = new Thickness();

            if (value is int intValue)
            {
                if ((parameter?.ToString()).Eq("top"))
                    result.Top = intValue;
                else if ((parameter?.ToString()).Eq("right"))
                    result.Right = intValue;
                else if ((parameter?.ToString()).Eq("bottom"))
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
