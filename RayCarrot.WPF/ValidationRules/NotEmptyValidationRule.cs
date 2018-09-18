using System;
using System.Globalization;
using System.Windows.Controls;

namespace RayCarrot.WPF
{
    /// <summary>
    /// Validates if the value is not an empty <see cref="String"/> or just white space
    /// </summary>
    public class NotEmptyValidationRule : ValidationRule
    {
        /// <summary>
        /// Validates the value
        /// </summary>
        /// <param name="value">The value from the binding target to check</param>
        /// <param name="cultureInfo">The culture to use in this rule</param>
        /// <returns>The validation result</returns>
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            return String.IsNullOrWhiteSpace((value ?? String.Empty).ToString()) ? new ValidationResult(false, "Field is required.") : ValidationResult.ValidResult;
        }
    }
}