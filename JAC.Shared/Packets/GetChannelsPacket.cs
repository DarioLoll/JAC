namespace JAC.Shared.Packets;

public class GetChannelsPacket : PacketBase
{
    public string? Query { get; init; }
}