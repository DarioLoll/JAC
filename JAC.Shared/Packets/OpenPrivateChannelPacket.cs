namespace JAC.Shared.Packets;

public class OpenPrivateChannelPacket : PacketBase
{
    public required string Username { get; init; }
}