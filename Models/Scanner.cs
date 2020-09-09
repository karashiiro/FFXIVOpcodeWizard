using FFXIVOpcodeWizard.PacketDetection;
using System;

namespace FFXIVOpcodeWizard.Models
{
    public class Scanner
    {
        public ushort Opcode { get; set; }

        public string PacketName { get; set; }
        public string Tutorial { get; set; }
        public Func<MetaPacket, string[], bool> ScanDelegate { get; set; }
        public PacketSource PacketSource { get; set; }
        public string[] ParameterPrompts { get; set; }
    }
}