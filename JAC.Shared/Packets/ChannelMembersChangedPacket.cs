namespace JAC.Shared.Packets;

/// <summary>
/// Packet to notify clients that the members of a channel have changed.
/// </summary>
public class ChannelMembersChangedPacket : PacketBase
{
    /// <summary>
    /// The id of the channel that the members changed in.
    /// </summary>
    public required ulong ChannelId { get; init; }
    
    /// <summary>
    /// The user that the change is about.
    /// </summary>
    public required UserModel User { get; init; }
    
    /// <summary>
    /// The type of change that occurred.
    /// </summary>
    public required ChannelMemberChangeType ChangeType { get; init; } 
}

/// <summary>
/// The type of change that occured to a channel member.
/// </summary>
public enum ChannelMemberChangeType
{
    Joined,
    Left,
    RankChanged
}