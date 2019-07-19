using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using RayCarrot.CarrotFramework;

namespace RayCarrot.WPF
{
    // TODO: Move to behavior
    /// <summary>
    /// A <see cref="TextBox"/> which only accepts decimals
    /// </summary>
    public class DecimalTextBox : TextBox
    {
        #region Constructor

        public DecimalTextBox()
        {
            TextChanged += IntTextBox_TextChanged;
            MaxLength = 9;
        }

        #endregion

        #region Private Fields

        private decimal _lastValue = -1;

        #endregion

        #region Event Handlers

        private void IntTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var caretIndex = CaretIndex;

            Value = Value;

            CaretIndex = caretIndex;
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

                var text = Text;

                var indexes = text.AllIndexesOf(separator).ToArray();

                var caretIndex = CaretIndex;

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
                Text = value.ToString(CultureInfo.CurrentCulture);
            }
        }

        #endregion
    }
}