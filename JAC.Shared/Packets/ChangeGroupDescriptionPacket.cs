namespace JAC.Shared.Packets;

/// <summary>
/// Packet for changing the description of a group.
/// </summary>
public class ChangeGroupDescriptionPacket : PacketBase
{
    /// <summary>
    /// The id of the group to change the description of.
    /// </summary>
    public required ulong ChannelId { get; init; }
    
    /// <summary>
    /// The new description of the group.
    /// </summary>
    public required string Description { get; init; }
}