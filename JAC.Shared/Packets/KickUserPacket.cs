namespace JAC.Shared.Packets;

public class KickUserPacket : PacketBase
{
    public required ulong ChannelId { get; init; }
    public required string Username { get; init; }
}