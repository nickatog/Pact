using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Data;

namespace Pact.Converters
{
    public sealed class SelectedViewMatchesName
        : IValueConverter
    {
        object IValueConverter.Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            var regex = new Regex($".*{parameter.ToString()}ViewModel.*", RegexOptions.IgnoreCase);

            return regex.IsMatch(value.GetType().Name);
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
