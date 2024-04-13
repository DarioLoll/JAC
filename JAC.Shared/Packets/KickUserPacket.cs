namespace JAC.Shared.Packets;


/// <summary>
/// Packet to kick a user from a channel.
/// </summary>
public class KickUserPacket : PacketBase
{
    /// <summary>
    /// The id of the channel to kick the user from.
    /// </summary>
    public required ulong ChannelId { get; init; }
    
    /// <summary>
    /// The username of the user to kick.
    /// </summary>
    public required string Username { get; init; }
}