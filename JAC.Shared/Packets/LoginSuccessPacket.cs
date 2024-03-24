using System.Runtime.Serialization;
using System.Text.Json;

namespace JAC.Shared.Packets;

public class LoginSuccessPacket : PacketBase
{
    public required LoginPacket Request { get; init; }
}