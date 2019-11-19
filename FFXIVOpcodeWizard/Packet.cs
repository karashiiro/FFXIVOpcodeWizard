namespace FFXIVOpcodeWizard
{
    class Packet
    {
        public string Connection;
        public long Epoch;
        public byte[] Data;
        public string Direction;

        public Packet(string connection, long epoch, byte[] data, string direction)
        {
            Connection = connection;
            Epoch = epoch;
            Data = data;
            Direction = direction;
        }
    }

    class MetaPacket
    {
        public string Connection;
        public long Epoch;
        public byte[] Data;
        public string Direction;

        public uint PacketSize;
        public ushort SegmentType;
        public ushort Opcode;

        public MetaPacket(Packet packet)
        {
            Connection = packet.Connection;
            Epoch = packet.Epoch;
            Data = packet.Data;
            Direction = packet.Direction;
        }
    }
}
