namespace JAC.Shared.Packets;

public class AddUserToGroupPacket : PacketBase
{
    public required string Username { get; init; }
    public required ulong ChannelId { get; init; }
}