using System.Windows;

namespace RayCarrot.WPF
{
    /// <summary>
    /// Interaction logic for BrowseBox.xaml
    /// </summary>
    public partial class BrowseBox : BrowseControl
    {
        #region Constructor

        public BrowseBox()
        {
            InitializeComponent();
            ControlRoot.DataContext = this;
            //var binding = new Binding()
            //{

            //};
            //binding.ValidationRules.Add();
            //MainTextField.SetBinding(TextBox.TextProperty, binding);
        }

        #endregion

        #region Methods

        private async void BrowseFileAsync(object sender, RoutedEventArgs e)
        {
            await BrowseAsync();
        }

        private async void ContextMenu_OpenLocationAsync(object sender, RoutedEventArgs e)
        {
            await OpenLocationAsync();
        }

        private void BrowseButton_DragEnter(object sender, DragEventArgs e)
        {
            e.Effects = AllowFileDragDrop() && e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        }

        private void BrowseButton_Drop(object sender, DragEventArgs e)
        {
            FileDrop(e);
        }

        #endregion
    }
}