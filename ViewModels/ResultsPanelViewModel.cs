using FFXIVOpcodeWizard.PacketDetection;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

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
                if (scanner.Opcode == 0) continue;

                sb.Append(scanner.PacketName).Append(" = ")
                    .Append(Util.NumberToString(scanner.Opcode, format)).Append(",").Append(Affix);
                if (scanner.Comment.Text != null)
                {
                    sb.Append(" (").Append(scanner.Comment).Append(")");
                }

                sb.AppendLine();
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