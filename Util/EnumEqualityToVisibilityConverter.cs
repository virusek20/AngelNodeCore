using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AngelNode.Util
{
    public class EnumEqualityToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null) return false;
            var checkValue = value.ToString();
            var targetValue = parameter.ToString();
            var equals = checkValue.Equals(targetValue, StringComparison.InvariantCultureIgnoreCase);

            return equals ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
