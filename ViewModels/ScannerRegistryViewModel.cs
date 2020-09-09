using FFXIVOpcodeWizard.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FFXIVOpcodeWizard.ViewModels
{
    public class ScannerRegistryViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Scanner> Scanners { get; set; }

        private Scanner selectedScanner;
        public Scanner SelectedScanner
        {
            get => this.selectedScanner;
            set
            {
                this.selectedScanner = value;
                OnPropertyChanged();
            }
        }

        public void Load(ScannerRegistry source)
        {
            Scanners = new ObservableCollection<Scanner>(source.AsList());
            this.selectedScanner = Scanners[0];
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}