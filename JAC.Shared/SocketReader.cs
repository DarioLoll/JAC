using System.Net.Sockets;
using System.Text;

namespace JAC.Shared;

public class SocketReader
{
    private readonly Socket _socket;
    
    private string _cache = string.Empty;
    
    private const int BufferSize = 8092;

    public SocketReader(Socket socket)
    {
        _socket = socket;
    }
    
    public async Task<string> Read()
    {
        return await Read(_socket);
    }
    
    public static async Task<string> Read(Socket socket)
    {
        var buffer = new byte[BufferSize];
        await socket.ReceiveAsync(buffer);
        var message = Encoding.ASCII.GetString(buffer).Trim('\0');
        
        return message;
    }

}