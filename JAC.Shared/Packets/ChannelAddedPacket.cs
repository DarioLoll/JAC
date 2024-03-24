using JAC.Shared.Channels;

namespace JAC.Shared.Packets;

public class ChannelAddedPacket : PacketBase
{
    public required IChannel NewChannel { get; init; }
}