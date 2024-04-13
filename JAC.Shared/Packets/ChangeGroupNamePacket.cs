namespace JAC.Shared.Packets;

/// <summary>
/// Packet for changing the name of a group.
/// </summary>
public class ChangeGroupNamePacket : PacketBase
{
    /// <summary>
    /// The id of the group to change the name of.
    /// </summary>
    public required ulong ChannelId { get; init; }
    
    /// <summary>
    /// The new name of the group.
    /// </summary>
    public required string NewName { get; init; }
}