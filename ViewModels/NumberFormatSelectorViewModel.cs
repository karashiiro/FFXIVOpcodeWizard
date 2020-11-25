using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using FFXIVOpcodeWizard.Models;

namespace FFXIVOpcodeWizard.ViewModels
{
    public class NumberFormatSelectorViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<RadioItem> NumberFormats { get; set; }

        private NumberDisplayFormat selectedFormat = NumberDisplayFormat.HexadecimalUppercase;
        public NumberDisplayFormat SelectedFormat
        {
            get => this.selectedFormat;
            private set
            {
                this.selectedFormat = value;
                OnPropertyChanged();
            }
        }

        public void Load()
        {
            NumberFormats = new ObservableCollection<RadioItem>
            {
                new RadioItem { Text = "Decimal", PropertyChanged = SetSelected },
                new RadioItem { Text = "Uppercase Hex", IsChecked = true, PropertyChanged = SetSelected },
                new RadioItem { Text = "Lowercase Hex", PropertyChanged = SetSelected },
            };
        }

        private void SetSelected(RadioItem ri)
        {
            SelectedFormat = GetCheckedFormat();
        }

        private NumberDisplayFormat GetCheckedFormat()
        {
            var selection = NumberFormats.First(cm => cm.IsChecked).Text;
            return selection switch
            {
                "Decimal" => NumberDisplayFormat.Decimal,
                "Uppercase Hex" => NumberDisplayFormat.HexadecimalUppercase,
                "Lowercase Hex" => NumberDisplayFormat.HexadecimalLowercase,
                _ => throw new NotImplementedException(),
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}