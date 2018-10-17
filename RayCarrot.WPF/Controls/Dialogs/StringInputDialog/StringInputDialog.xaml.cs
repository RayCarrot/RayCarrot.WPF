using System.Windows;

namespace RayCarrot.WPF
{
    /// <summary>
    /// Interaction logic for StringInputDialog.xaml
    /// </summary>
    public partial class StringInputDialog : DialogVMWindow<StringInputViewModel, StringInputResult>
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="vm">The view model</param>
        public StringInputDialog(StringInputViewModel vm) : base(vm)
        {
            InitializeComponent();
            CanceledByUser = true;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Indicates if the dialog was canceled by the user, default is true
        /// </summary>
        public bool CanceledByUser { get; set; }

        #endregion

        #region Protected Overrides

        /// <summary>
        /// Gets the current result for the dialog
        /// </summary>
        /// <returns>The result</returns>
        protected override StringInputResult GetResult()
        {
            return new StringInputResult()
            {
                CanceledByUser = CanceledByUser,
                StringInput = ViewModel.StringInput
            };
        }

        #endregion

        #region Event Handlers

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            CanceledByUser = false;

            // Close the dialog
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Close the dialog
            Close();
        }

        #endregion
    }
}