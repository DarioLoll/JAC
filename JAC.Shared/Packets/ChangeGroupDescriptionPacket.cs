namespace JAC.Shared.Packets;

public class ChangeGroupDescriptionPacket : PacketBase
{
    public required ulong ChannelId { get; init; }
    public required string Description { get; init; }
}