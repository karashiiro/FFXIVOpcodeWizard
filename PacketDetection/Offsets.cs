namespace FFXIVOpcodeWizard.PacketDetection
{
    // https://github.com/SapphireServer/Sapphire/blob/master/src/common/Network/CommonNetwork.h
    public static class Offsets
    {
        public const int PacketSize = 0x00;
        public const int SourceActor = 0x04;
        public const int TargetActor = 0x08;
        public const int SegmentType = 0x0C;
        public const int IpcType = 0x12;
        public const int ServerId = 0x16;
        public const int Timestamp = 0x18;
        public const int IpcData = 0x20;
    }
}
