using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using JAC.Shared.Packets;

namespace JAC.Shared;

public class SocketWriter
{
    private readonly Socket _socket;

    public SocketWriter(Socket socket)
    {
        _socket = socket;
    }
    
    public async Task Send(string? message)
    {
        if (string.IsNullOrEmpty(message)) return;
        var buffer = Encoding.ASCII.GetBytes(message);
        await _socket.SendAsync(buffer);
    }
    
    public async Task Send(PacketBase packet)
    {
        await Send(_socket, packet);
    }
    
    public static async Task Send(Socket socket, string? message)
    {
        if (string.IsNullOrEmpty(message)) return;
        var buffer = Encoding.ASCII.GetBytes(message);
        await socket.SendAsync(buffer);
    }
    
    public static async Task Send(Socket socket, PacketBase packet)
    {
        Type packetType = packet.GetType();
        var prefix = packet.ParameterlessPacketType == null 
            ? PacketBase.GetPrefix(packetType) 
            : PacketBase.GetPrefix(packet.ParameterlessPacketType.Value);
        string message = $"{prefix} {JsonSerializer.Serialize(packet, packetType)}";
        await Send(socket, message);
    }
}