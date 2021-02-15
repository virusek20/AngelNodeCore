using System.Globalization;
using System.Windows.Controls;

namespace AngelNode.Util.Validation
{
    public class PositiveFloatRule : ValidationRule
    {
        public bool AllowZero { get; set; } = true;

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (!float.TryParse((string)value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out float num)) return new ValidationResult(false, "Value is not a number.");

            if (AllowZero)
            {
                return num < 0 ? new ValidationResult(false, "Value has to be a non-negative float.") : ValidationResult.ValidResult;
            }

            return num <= 0 ? new ValidationResult(false, "Value has to be a positive float.") : ValidationResult.ValidResult;
        }
    }
}
