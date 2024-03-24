namespace JAC.Shared.Packets;

public class ChannelNameChangedPacket : PacketBase
{
    public required ulong ChannelId { get; init; }
    public required string NewName { get; init; }
}