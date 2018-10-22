using System;
using System.Windows.Media;

namespace Pact
{
    public sealed class ClassNameToBrushConverter
        : System.Windows.Data.IValueConverter
    {
        public object Convert(
         object value,
         Type targetType,
         object parameter,
         System.Globalization.CultureInfo culture)
        {
            string text;
            if ((text = value as string) == null)
                return null;

            if (string.Equals(text, "druid", StringComparison.OrdinalIgnoreCase))
                return new SolidColorBrush(Color.FromRgb(255, 125, 10));
            else if (string.Equals(text, "hunter", StringComparison.OrdinalIgnoreCase))
                return new SolidColorBrush(Color.FromRgb(171, 212, 115));
            else if (string.Equals(text, "mage", StringComparison.OrdinalIgnoreCase))
                return new SolidColorBrush(Color.FromRgb(105, 204, 240));
            else if (string.Equals(text, "paladin", StringComparison.OrdinalIgnoreCase))
                return new SolidColorBrush(Color.FromRgb(245, 140, 186));
            else if (string.Equals(text, "priest", StringComparison.OrdinalIgnoreCase))
                return new SolidColorBrush(Color.FromRgb(255, 255, 255));
            else if (string.Equals(text, "rogue", StringComparison.OrdinalIgnoreCase))
                return new SolidColorBrush(Color.FromRgb(255, 245, 105));
            else if (string.Equals(text, "shaman", StringComparison.OrdinalIgnoreCase))
                return new SolidColorBrush(Color.FromRgb(0, 112, 222));
            else if (string.Equals(text, "warlock", StringComparison.OrdinalIgnoreCase))
                return new SolidColorBrush(Color.FromRgb(148, 130, 201));
            else if (string.Equals(text, "warrior", StringComparison.OrdinalIgnoreCase))
                return new SolidColorBrush(Color.FromRgb(199, 156, 110));

            return new SolidColorBrush(Color.FromRgb(80, 80, 80));
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
