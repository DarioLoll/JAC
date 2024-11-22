using JAC.Shared.Channels;

namespace JAC.Shared.Packets;

/// <summary>
/// The packet that is sent to the client when the login is successful.
/// </summary>
public class LoginSuccessPacket : PacketBase
{
    /// <summary>
    /// The user that has logged in.
    /// </summary>
    public required UserProfile User { get; init; }
    
    /// <summary>
    /// Contains the list of channels that the client's user is a member of.
    /// </summary>
    public required IEnumerable<ChannelProfileBase> Channels { get; init; }
}