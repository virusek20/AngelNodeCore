using AngelNode.Model.Node;
using System;
using System.Globalization;
using System.Windows.Data;

namespace AngelNode.Util
{
    public class OperationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is NodeSetVariable.SetType setType)
            {
                switch (setType)
                {
                    case NodeSetVariable.SetType.Add:
                        return "+=";
                    case NodeSetVariable.SetType.Set:
                        return "=";
                    default:
                        return "???";
                }
            }

            if (value is NodeVariableJump.ComparisonType comparisonType)
            {
                switch (comparisonType)
                {
                    case NodeVariableJump.ComparisonType.Equal:
                        return "==";
                    case NodeVariableJump.ComparisonType.GreaterThan:
                        return ">";
                    case NodeVariableJump.ComparisonType.LessThan:
                        return "<";
                    default:
                        return "???";
                }
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
