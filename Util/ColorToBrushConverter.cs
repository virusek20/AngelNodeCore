using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace AngelNode.Util
{
    public class ColorToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is Color color)) return Brushes.Transparent;

            return new SolidColorBrush(Color.FromArgb(color.A, color.R, color.G, color.B));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
