using System;
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

        private string affix;
        public string Affix
        {
            get => this.affix;
            set
            {
                this.affix = value;
                OnPropertyChanged();
            }
        }

        public void Load()
        {
            this.affix = "";
            this.contents = "";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}