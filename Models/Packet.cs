﻿using FFXIVOpcodeWizard.PacketDetection;

namespace FFXIVOpcodeWizard.Models
{
    public class Packet
    {
        public string Connection { get; set; }
        public long Epoch { get; set; }
        public byte[] Data { get; set; }
        public PacketSource Source { get; set; }
    }

    public class MetaPacket : Packet
    {
        public uint PacketSize { get; set; }
        public ushort SegmentType { get; set; }
        public ushort Opcode { get; set; }
    }
}
