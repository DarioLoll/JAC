using JAC.Shared.Channels;

namespace JAC.Shared.Packets;

/// <summary>
/// Packet to respond to a client's request for a list of channels.
/// <seealso cref="JAC.Shared.ParameterlessPacket"/>
/// </summary>
public class GetChannelsResponsePacket : PacketBase
{
    /// <summary>
    /// Contains the list of channels that the client's user is a member of.
    /// </summary>
    public required IEnumerable<ChannelProfileBase> Channels { get; init; }
}