using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Pact
{
    public sealed class ClassNameToBrushConverter
        : IValueConverter
    {
        private readonly static IDictionary<string, Brush> s_classColors =
            new Dictionary<string, Brush>(StringComparer.OrdinalIgnoreCase)
            {
                { "druid", new SolidColorBrush(Color.FromRgb(255, 125, 10)) },
                { "hunter", new SolidColorBrush(Color.FromRgb(171, 212, 115)) },
                { "mage", new SolidColorBrush(Color.FromRgb(105, 204, 240)) },
                { "paladin", new SolidColorBrush(Color.FromRgb(245, 140, 186)) },
                { "priest", new SolidColorBrush(Color.FromRgb(255, 255, 255)) },
                { "rogue", new SolidColorBrush(Color.FromRgb(255, 245, 105)) },
                { "shaman", new SolidColorBrush(Color.FromRgb(0, 112, 222)) },
                { "warlock", new SolidColorBrush(Color.FromRgb(148, 130, 201)) },
                { "warrior", new SolidColorBrush(Color.FromRgb(199, 156, 110)) }
            };

        public object Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            if (!(value is string text))
                return null;

            if (text != null && s_classColors.TryGetValue(text, out Brush classColor))
                return classColor;

            return new SolidColorBrush(Color.FromRgb(80, 80, 80));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
