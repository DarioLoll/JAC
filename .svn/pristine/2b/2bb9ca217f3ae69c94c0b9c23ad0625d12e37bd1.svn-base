using System.Net.Sockets;
using System.Text;

namespace JAC.Shared;

public class SocketReader
{
    private readonly Socket _socket;

    public SocketReader(Socket socket)
    {
        _socket = socket;
    }
    
    public async Task<string> Read()
    {
        var buffer = new byte[1024];
        await _socket.ReceiveAsync(buffer);
        return Encoding.ASCII.GetString(buffer).Trim('\0');
    }
    
    public static async Task<string> Read(Socket socket)
    {
        var buffer = new byte[1024];
        await socket.ReceiveAsync(buffer);
        return Encoding.ASCII.GetString(buffer).Trim('\0');
    }
}