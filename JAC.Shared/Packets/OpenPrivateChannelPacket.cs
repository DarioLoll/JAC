namespace JAC.Shared.Packets;

/// <summary>
/// Packet to open a private channel with a user. (private conversation)
/// </summary>
public class OpenPrivateChannelPacket : PacketBase
{
    /// <summary>
    /// The username of the user to open a private channel with.
    /// </summary>
    public required string Username { get; init; }
}