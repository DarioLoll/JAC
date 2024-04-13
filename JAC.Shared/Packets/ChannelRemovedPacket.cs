namespace JAC.Shared.Packets;

/// <summary>
/// Packet to notify clients that their user has been removed from a channel.
/// </summary>
public class ChannelRemovedPacket : PacketBase
{
    /// <summary>
    /// The id of the channel that the user was removed from.
    /// </summary>
    public required ulong RemovedChannelId { get; init; }
}