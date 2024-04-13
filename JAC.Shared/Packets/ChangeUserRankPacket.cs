namespace JAC.Shared.Packets;

/// <summary>
/// Packet for changing the rank of a user in a group.
/// <remarks>Currently, there are only two ranks: Member and Admin,
/// so this packet will change the demote the user, if they are an admin and promote them if they are a member</remarks>
/// </summary>
public class ChangeUserRankPacket : PacketBase
{
    /// <summary>
    /// The username of the user to change the rank of.
    /// </summary>
    public required string Username { get; init; }
    
    /// <summary>
    /// The id of the group to change the rank in.
    /// </summary>
    public required ulong ChannelId { get; init; }
    
}
