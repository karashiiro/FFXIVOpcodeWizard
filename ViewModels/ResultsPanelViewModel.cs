using System.ComponentModel;
using System.Runtime.CompilerServices;

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

        private string addendum;
        public string Addendum
        {
            get => this.addendum;
            set
            {
                this.addendum = value;
                OnPropertyChanged();
            }
        }

        public void Load()
        {
            this.addendum = "";
            this.contents = "";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}