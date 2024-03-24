namespace JAC.Shared.Packets;

public class MessageReceivedPacket : PacketBase
{
    public required ulong ChannelId { get; init; }
    public required Message Message { get; init; }
}