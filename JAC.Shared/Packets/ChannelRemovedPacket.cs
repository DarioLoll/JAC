namespace JAC.Shared.Packets;

public class ChannelRemovedPacket : PacketBase
{
    public required ulong RemovedChannelId { get; init; }
}