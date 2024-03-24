namespace JAC.Shared.Packets;

public class ChangeUserRankPacket : PacketBase
{
    public required string Username { get; init; }
    public required ulong ChannelId { get; init; }
}