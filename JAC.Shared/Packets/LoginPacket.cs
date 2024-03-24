using System.Text.Json;
using System.Text.Json.Serialization;

namespace JAC.Shared.Packets;

public class LoginPacket : PacketBase
{
    public required string Username { get; init; }
    
}