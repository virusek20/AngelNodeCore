using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Data;
using AngelNode.Model.Project;

namespace AngelNode.Util
{
    class MessageSeverityToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            var severity = (ProjectReportMessage.MessageSeverity) value;

            switch (severity)
            {
                case ProjectReportMessage.MessageSeverity.Debug:
                    return SystemIcons.Application.ToImageSource();
                case ProjectReportMessage.MessageSeverity.Info:
                    return SystemIcons.Information.ToImageSource();
                case ProjectReportMessage.MessageSeverity.Warning:
                    return SystemIcons.Exclamation.ToImageSource();
                case ProjectReportMessage.MessageSeverity.Error:
                    return SystemIcons.Error.ToImageSource();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
