namespace JAC.Shared.Packets;

/// <summary>
/// Packet to notify clients that the name of a channel has changed.
/// </summary>
public class ChannelNameChangedPacket : PacketBase
{
    /// <summary>
    /// The id of the channel that the name changed in.
    /// </summary>
    public required ulong ChannelId { get; init; }
    
    /// <summary>
    /// The new name of the channel.
    /// </summary>
    public required string NewName { get; init; }
}