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
        var buffer = Encoding.ASCII.GetBytes(message);
        await _socket.SendAsync(buffer);
    }
}