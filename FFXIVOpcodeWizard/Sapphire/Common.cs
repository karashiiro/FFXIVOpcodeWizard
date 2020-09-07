namespace Sapphire.Common.Network
{
    // https://github.com/SapphireServer/Sapphire/blob/master/src/common/Network/CommonNetwork.h
    public enum Offsets
    {
        PacketSize = 0x00,
        SourceActor = 0x04,
        TargetActor = 0x08,
        SegmentType = 0x0C,
        IpcType = 0x12,
        ServerId = 0x16,
        Timestamp = 0x18,
        IpcData = 0x20,
    }
}
