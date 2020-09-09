using System.Windows.Controls;

namespace FFXIVOpcodeWizard.Views
{
    /// <summary>
    /// Interaction logic for ScannerList.xaml
    /// </summary>
    public partial class ScannerList
    {
        public ScannerList()
        {
            InitializeComponent();
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBox = (ListBox)e.Source;
            listBox.ScrollIntoView(listBox.SelectedItem);
        }
    }
}
