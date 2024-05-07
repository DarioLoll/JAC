using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using JAC.Shared.Packets;

namespace JAC.Shared;

public class SocketWriter
{
    private readonly Socket _socket;
    
    private const int MaxCharacters = 4096; // 8092 Bytes, 1 Character = 2 Bytes


    public SocketWriter(Socket socket)
    {
        _socket = socket;
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
        var json = JsonSerializer.Serialize(packet, packetType);
        string packetAsString = $"{prefix} {json.Length + 1} {json}";
        if (packetAsString.Length >= MaxCharacters)
        {
            // Fragment the packet if it's too large
            var fragmentSize = MaxCharacters - 500;
            var fragments = packetAsString.FragmentIntoPackets(fragmentSize);
            foreach (var fragment in fragments)
            {
                Console.WriteLine($"Sending fragment {fragment.SequenceNumber} of {fragment.Id}");
                await Send(socket, fragment);
            }
        }
        else await Send(socket, packetAsString);
    }
}