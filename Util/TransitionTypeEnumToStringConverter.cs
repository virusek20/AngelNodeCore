using System;
using System.Globalization;
using System.Windows.Data;
using AngelNode.Model.Node;

namespace AngelNode.Util
{
    public class TransitionTypeEnumToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            var transitionType = (NodeChangeBackground.BackgroundTransitionTypeEnum) value;

            return transitionType switch
            {
                NodeChangeBackground.BackgroundTransitionTypeEnum.Blend => "Blend",
                NodeChangeBackground.BackgroundTransitionTypeEnum.NewDay => "New day",
                NodeChangeBackground.BackgroundTransitionTypeEnum.Instant => "Instant",
                NodeChangeBackground.BackgroundTransitionTypeEnum.FadeToBlack => "Fade to black",
                _ => throw new ArgumentOutOfRangeException(nameof(value))
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
