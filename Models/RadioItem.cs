using System;

namespace FFXIVOpcodeWizard.Models
{
    public class RadioItem
    {
        private string text;
        public string Text
        {
            get => this.text;
            set
            {
                this.text = value;
                PropertyChanged?.Invoke(this);
            }
        }

        private bool isChecked;
        public bool IsChecked
        {
            get => this.isChecked;
            set
            {
                this.isChecked = value;
                PropertyChanged?.Invoke(this);
            }
        }

        public Action<RadioItem> PropertyChanged { get; set; }
    }
}