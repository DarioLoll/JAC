namespace JAC.Shared.Packets;

/// <summary>
/// Packet for creating a new group.
/// </summary>
public class CreateGroupPacket : PacketBase
{
    /// <summary>
    /// The name of the group.
    /// </summary>
    public required string Name { get; init; }
    
    /// <summary>
    /// The description of the group (optional).
    /// </summary>
    public string Description { get; init; } = string.Empty;

}