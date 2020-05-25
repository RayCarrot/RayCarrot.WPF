using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;
using RayCarrot.Common;

namespace RayCarrot.WPF
{
    /// <summary>
    /// Behavior for a <see cref="TextBox"/> which limits it to only accept decimals
    /// </summary>
    public class DecimalTextBoxBehavior : Behavior<TextBox>
    {
        #region Overrides

        protected override void OnAttached()
        {
            AssociatedObject.TextChanged += IntTextBox_TextChanged;
            AssociatedObject.MaxLength = 9;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.TextChanged -= IntTextBox_TextChanged;
        }

        #endregion

        #region Private Fields

        private decimal _lastValue = -1;

        #endregion

        #region Event Handlers

        private void IntTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Save the caret index
            var caretIndex = AssociatedObject.CaretIndex;

            // Refresh the value
            Value = Value;

            // Restore the caret index
            AssociatedObject.CaretIndex = caretIndex;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The value of the int text box as a <see cref="decimal"/>
        /// </summary>
        public decimal Value
        {
            get
            {
                var separator = Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator;

                var text = AssociatedObject.Text;

                var indexes = text.AllIndexesOf(separator).ToArray();

                var caretIndex = AssociatedObject.CaretIndex;

                if (indexes.Any())
                {
                    if (indexes.Length == 1)
                    {
                        if (indexes.First() == text.Length - 1)
                            text += 0;
                    }
                    else
                    {
                        text = _lastValue.ToString(CultureInfo.CurrentCulture);
                    }
                }
                else
                {
                    text = text.Insert(caretIndex, separator);

                    if (caretIndex == text.Length - 1)
                        text += 0;
                }

                return Decimal.TryParse(text, out decimal output) ? output : _lastValue;
            }
            set
            {
                _lastValue = value;
                AssociatedObject.Text = value.ToString(CultureInfo.CurrentCulture);
            }
        }

        #endregion
    }
}