using System;
using System.Globalization;
using System.Windows.Data;
using AngelNode.Model.Node;

namespace AngelNode.Util
{
    class EventTypeEnumToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !(value is NodeEvent.EventTypeEnum)) return null;
            var eventType = (NodeEvent.EventTypeEnum) value;

            return eventType switch
            {
                NodeEvent.EventTypeEnum.Call => "Phone call",
                NodeEvent.EventTypeEnum.Contacts => "Phone contacts",
                NodeEvent.EventTypeEnum.Custom => "Custom effect",
                NodeEvent.EventTypeEnum.HookBrackets => "Hook brackets text",
                NodeEvent.EventTypeEnum.CallEnd => "End call",
                _ => throw new ArgumentOutOfRangeException(),
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
