namespace JAC.Shared.Packets;

public class ChannelDescriptionChangedPacket : PacketBase
{
    public required ulong ChannelId { get; init; }
    public required string NewDescription { get; init; }
}