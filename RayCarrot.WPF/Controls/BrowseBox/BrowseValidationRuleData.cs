using System.Windows;

namespace RayCarrot.WPF
{
    /// <summary>
    /// Validation rule data for <see cref="BrowseBoxValidationRule"/>
    /// </summary>
    public class BrowseValidationRuleData : DependencyObject
    {
        /// <summary>
        /// The validation rule to use
        /// </summary>
        public BrowseValidationRules ValidationRule
        {
            get => (BrowseValidationRules)GetValue(ValidationRuleProperty);
            set => SetValue(ValidationRuleProperty, value);
        }

        /// <summary>
        /// The dependency Validation for <see cref="ValidationRule"/>
        /// </summary>
        public static readonly DependencyProperty ValidationRuleProperty = DependencyProperty.Register(nameof(ValidationRule), typeof(BrowseValidationRules), typeof(BrowseValidationRuleData));
    }
}