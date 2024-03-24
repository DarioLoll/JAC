namespace JAC.Shared.Packets;

public class LeaveGroupPacket : PacketBase
{
    public required ulong ChannelId { get; init; }
}