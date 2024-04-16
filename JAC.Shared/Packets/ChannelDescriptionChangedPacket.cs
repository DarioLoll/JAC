namespace JAC.Shared.Packets;

/// <summary>
/// Packet to notify clients that a channel's description has changed.
/// </summary>
public class ChannelDescriptionChangedPacket : PacketBase
{
    /// <summary>
    /// The id of the channel that the description changed in.
    /// </summary>
    public required ulong ChannelId { get; init; }
    
    /// <summary>
    /// The new description of the channel.
    /// </summary>
    public required string NewDescription { get; init; }
}