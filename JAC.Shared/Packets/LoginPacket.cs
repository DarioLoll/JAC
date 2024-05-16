namespace JAC.Shared.Packets;

/// <summary>
/// Packet to log in to the chat system.
/// </summary>
public class LoginPacket : PacketBase
{
    /// <summary>
    /// The username of the user to log in.
    /// <remarks>The username has to be unique</remarks>
    /// </summary>
    public required string Username { get; init; }
}