using System.Runtime.Serialization;
using System.Text.Json;

namespace JAC.Shared.Packets;

/// <summary>
/// The packet that is sent to the client when the login is successful.
/// </summary>
public class LoginSuccessPacket : PacketBase
{
    /// <summary>
    /// The user that has logged in.
    /// </summary>
    public required UserModel User { get; init; }
}