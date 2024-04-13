using JAC.Shared.Channels;

namespace JAC.Shared.Packets;

/// <summary>
/// Packet to notify clients that their user has been added to a channel.
/// </summary>
public class ChannelAddedPacket : PacketBase
{
    /// <summary>
    /// The channel that the user was added to.
    /// </summary>
    public required IChannel NewChannel { get; init; }
}