using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using FFXIVOpcodeWizard.PacketDetection;

namespace FFXIVOpcodeWizard.ViewModels
{
    public class ResultsPanelViewModel : INotifyPropertyChanged
    {
        private string contents;
        public string Contents
        {
            get => this.contents;
            set
            {
                this.contents = value;
                OnPropertyChanged();
            }
        }

        private string affix;
        public string Affix
        {
            get => this.affix;
            set
            {
                this.affix = value;
                OnPropertyChanged();
                UpdateContents();
            }
        }

        private ScannerRegistry registry;
        private NumberFormatSelectorViewModel numberFormatSelector;

        public void Load(ScannerRegistry registry, NumberFormatSelectorViewModel numberFormatSelector)
        {
            this.registry = registry;
            this.numberFormatSelector = numberFormatSelector;

            this.affix = "";
            this.contents = "";
        }

        public void UpdateContents()
        {
            var sb = new StringBuilder();

            var format = this.numberFormatSelector.SelectedFormat;

            foreach (var scanner in this.registry.AsList())
            {
                if (scanner.Opcode != 0)
                {
                    sb.AppendLine($"{scanner.PacketName} = {Util.NumberToString(scanner.Opcode, format)},{Affix}");
                }
            }

            Contents = sb.ToString();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}