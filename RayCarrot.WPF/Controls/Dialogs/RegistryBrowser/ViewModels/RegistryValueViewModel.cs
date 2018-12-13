using System;
using System.Linq;
using Microsoft.Win32;
using RayCarrot.CarrotFramework;

namespace RayCarrot.WPF
{
    /// <summary>
    /// View model for a Registry value
    /// </summary>
    public class RegistryValueViewModel : BaseViewModel
    {
        #region Public Properties

        /// <summary>
        /// The name of the value
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// The name of the value to display
        /// </summary>
        public virtual string DisplayName => IsDefault ? "(Default)" : Name;

        /// <summary>
        /// Indicates if the value is a default value
        /// </summary>
        public virtual bool IsDefault => Name == String.Empty;

        /// <summary>
        /// The value kind
        /// </summary>
        public virtual RegistryValueKind Type { get; set; }

        /// <summary>
        /// The value data
        /// </summary>
        public virtual object Data { get; set; }

        /// <summary>
        /// The data to display
        /// </summary>
        public virtual string DisplayData => GetDisplayData();

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the display data
        /// </summary>
        /// <returns>A string representing the data</returns>
        public virtual string GetDisplayData()
        {
            switch (Data)
            {
                case string s:
                {
                    // Remove invalid characters if not correctly null-terminated
                    if (s.Contains(Convert.ToChar(0x0).ToString()))
                        s = s.Substring(0, s.IndexOf(Convert.ToChar(0x0).ToString(), StringComparison.Ordinal));

                    // Return if a string
                    return s;
                }

                case null:
                    // Check if there is any data
                    return "(Value is not set)";

                case byte[] bytes:
                    // Convert to a hex string if a byte array
                    return BitConverter.ToString(bytes).Replace("-", " ");

                case Array array:
                    // Show blank space separated if an array
                    return array.Cast<object>().JoinItems(" ");

                default:
                    // Return as string
                    return Data.ToString();
            }
        }

        #endregion
    }
}