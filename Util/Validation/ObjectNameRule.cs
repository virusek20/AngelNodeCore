using System.Globalization;
using System.Windows.Controls;

namespace AngelNode.Util.Validation
{
    public class ObjectNameRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (string.IsNullOrWhiteSpace((string) value)) return new ValidationResult(false, "Name cannot be empty or entirely whitespace characters.");
            return ValidationResult.ValidResult;
        }
    }
}
