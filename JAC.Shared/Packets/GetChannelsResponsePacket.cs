using JAC.Shared.Channels;

namespace JAC.Shared.Packets;

public class GetChannelsResponsePacket : PacketBase
{
    public required List<IChannel> Channels { get; init; }
}