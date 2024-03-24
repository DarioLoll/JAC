namespace JAC.Shared.Packets;

public class SendMessagePacket : PacketBase
{
    public required ulong ChannelId { get; init; }
    public required string Message { get; init; }
}