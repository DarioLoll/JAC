using System.Net.Sockets;
using System.Text;

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
        Type packetType = packet.GetType();
        string message = PacketBase.GetPrefix(packetType) + " " + packet.ToJson();
        await Send(message);
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
        string message = PacketBase.GetPrefix(packetType) + " " + packet.ToJson();
        await Send(socket, message);
    }
}