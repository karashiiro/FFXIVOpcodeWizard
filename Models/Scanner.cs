using FFXIVOpcodeWizard.PacketDetection;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace FFXIVOpcodeWizard.Models
{
    public class Scanner : INotifyPropertyChanged
    {
        private ushort opcode;
        public ushort Opcode
        {
            get => this.opcode;
            set
            {
                this.opcode = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(WpfOpcodeFound));
            }
        }

        public Visibility WpfOpcodeFound => this.opcode switch
        {
            0 => Visibility.Hidden,
            _ => Visibility.Visible,
        };

        private bool running;
        public bool Running
        {
            get => this.running;
            set
            {
                this.running = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(WpfRunning));
            }
        }

        public Visibility WpfRunning => this.running switch
        {
            true => Visibility.Visible,
            false => Visibility.Hidden,
        };

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