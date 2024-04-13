namespace JAC.Shared.Packets;

/// <summary>
/// Packet to leave a group.
/// </summary>
public class LeaveGroupPacket : PacketBase
{
    /// <summary>
    /// The id of the channel to leave.
    /// </summary>
    public required ulong ChannelId { get; init; }
}