namespace JAC.Shared.Packets;

/// <summary>
/// Packet for adding a user to a group.
/// </summary>
public class AddUserToGroupPacket : PacketBase
{
    /// <summary>
    /// The username of the user to add.
    /// </summary>
    public required string Username { get; init; }
    
    /// <summary>
    /// The id of the group to add the user to.
    /// </summary>
    public required ulong ChannelId { get; init; }
}