using System;
using FFXIVOpcodeWizard.Models;
using FFXIVOpcodeWizard.PacketDetection;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace FFXIVOpcodeWizard.ViewModels
{
    public class ScannerRegistryViewModel : INotifyPropertyChanged
    {
        public ReadOnlyObservableCollection<Scanner> Scanners { get; set; }

        private Scanner selectedScanner;
        public Scanner SelectedScanner
        {
            get => this.selectedScanner;
            set
            {
                if (this.selectedScanner == value) return;
                this.selectedScanner = value;
                OnPropertyChanged();
            }
        }

        public Action<object> RunOneCommand { get; set; }
        public Action<object> RunFromHereCommand { get; set; }

        public ICommand WpfRunOneCommand => new RelayCommand(RunOneCommand);
        public ICommand WpfRunFromHereCommand => new RelayCommand(RunFromHereCommand);

        public void Load(ScannerRegistry source)
        {
            Scanners = new ReadOnlyObservableCollection<Scanner>(
                new ObservableCollection<Scanner>(source.AsList()));
            this.selectedScanner = Scanners[0];
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}