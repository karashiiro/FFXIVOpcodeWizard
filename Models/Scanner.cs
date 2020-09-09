using FFXIVOpcodeWizard.PacketDetection;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FFXIVOpcodeWizard.Models
{
    public class Scanner : INotifyPropertyChanged
    {
        public ushort Opcode { get; set; }

        private bool running;
        public bool Running
        {
            get => this.running;
            set
            {
                this.running = value;
                OnPropertyChanged();
            }
        }

        public string PacketName { get; set; }
        public string Tutorial { get; set; }
        public Func<MetaPacket, string[], bool> ScanDelegate { get; set; }
        public PacketSource PacketSource { get; set; }
        public string[] ParameterPrompts { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}