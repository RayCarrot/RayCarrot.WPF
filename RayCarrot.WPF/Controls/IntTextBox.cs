using System;
using System.Windows.Controls;

namespace RayCarrot.WPF
{
    // TODO: Move to behavior
    /// <summary>
    /// A <see cref="TextBox"/> which only accepts integers
    /// </summary>
    public class IntTextBox : TextBox
    {
        #region Constructor

        public IntTextBox()
        {
            TextChanged += IntTextBox_TextChanged;
            MaxLength = 9;
        }

        #endregion

        #region Private Fields

        private int _lastValue = -1;

        #endregion

        #region Event Handlers

        private void IntTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Value = Value;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The value of the int text box as an <see cref="Int32"/>
        /// </summary>
        public int Value
        {
            get => Int32.TryParse(Text, out int output) ? output : _lastValue;
            set
            {
                _lastValue = value;
                Text = value.ToString();
            }
        }

        #endregion
    }
}