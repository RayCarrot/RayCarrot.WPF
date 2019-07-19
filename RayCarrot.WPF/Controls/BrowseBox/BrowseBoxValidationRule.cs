using System;
using System.Globalization;
using System.IO;
using System.Windows.Controls;
using RayCarrot.Extensions;

namespace RayCarrot.WPF
{
    /// <summary>
    /// Validates if a value for a <see cref="BrowseBox"/>
    /// </summary>
    public class BrowseBoxValidationRule : ValidationRule
    {
        public BrowseBoxValidationRule()
        {
            ValidationData = new BrowseValidationRuleData();
        }

        /// <summary>
        /// Validates the value
        /// </summary>
        /// <param name="value">The value from the binding target to check</param>
        /// <param name="cultureInfo">The culture to use in this rule</param>
        /// <returns>The validation result</returns>
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string input = (value ?? String.Empty).ToString();

            switch (ValidationData.ValidationRule)
            {
                default:
                case BrowseValidationRules.None:
                    return ValidationResult.ValidResult;

                case BrowseValidationRules.NotEmpty:
                    return input.IsNullOrWhiteSpace() ? new ValidationResult(false, "Field is required") : ValidationResult.ValidResult;

                case BrowseValidationRules.FileExists:
                    return input.IsNullOrWhiteSpace() || File.Exists(input) ? ValidationResult.ValidResult : new ValidationResult(false, "The file does not exist");

                case BrowseValidationRules.FileExistsAndNotEmpty:
                    return !input.IsNullOrWhiteSpace() && File.Exists(input) ? ValidationResult.ValidResult : new ValidationResult(false, "The file does not exist");

                case BrowseValidationRules.DirectoryExists:
                    return input.IsNullOrWhiteSpace() || Directory.Exists(input) ? ValidationResult.ValidResult : new ValidationResult(false, "The directory does not exist");

                case BrowseValidationRules.DirectoryExistsAndNotEmpty:
                    return !input.IsNullOrWhiteSpace() && Directory.Exists(input) ? ValidationResult.ValidResult : new ValidationResult(false, "The directory does not exist");
            }
        }

        /// <summary>
        /// The validation data
        /// </summary>
        public BrowseValidationRuleData ValidationData { get; set; }
    }
}