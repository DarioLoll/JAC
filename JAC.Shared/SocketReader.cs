using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace JAC.Shared;

/// <summary>
/// Provides functionality for listening for incoming messages and extracting packets from them on a socket
/// </summary>
public class SocketReader
{
    private readonly Socket _socket;
    
    private string _cache = string.Empty;
    
    private const int BufferSize = 8092;
    
    /// <summary>
    /// Occurs when a whole packet could be constructed from the incoming messages
    /// </summary>
    public event Func<PacketBase, Task>? PacketReceived;
    
    /// <summary>
    /// Fires when an error occurs during the reading process
    /// </summary>
    public event Func<Exception, Task>? Error;

    public SocketReader(Socket socket)
    {
        _socket = socket;
    }
    
    /// <summary>
    /// Listens for incoming messages and extracts packets from them
    /// <remarks>When a packet has been received, the <see cref="PacketReceived"/> event is raised</remarks>
    /// </summary>
    /// <param name="cancellationToken">For cancelling the listening process</param>
    public async Task ListenAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var message = await Read(cancellationToken);
                if (string.IsNullOrEmpty(message)) continue;
                //extract packets from the message
                //this is not awaited because we want to continue listening for messages
                _ = Task.Run(() => ExtractPacketsAsync(message), CancellationToken.None);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception e)
            {
                OnError(e);
            }
        }
    }

    /// <summary>
    /// Attempts to extract packets from the message, if a packet is not complete, it is added to the cache
    /// </summary>
    private async Task ExtractPacketsAsync(string message)
    {
        while (true)
        {
            var splitMessage = message.Split(' ', 3);
            //A packet consists of 3 parts: prefix, length, data
            //If the message does not contain all 3 parts,
            //it is added to the cache because it is not a complete packet
            if (splitMessage.Length < 3)
            {
                await AddToCacheAsync(message);
                return;
            }
            
            var prefix = splitMessage[0];
            var length = int.Parse(splitMessage[1]);
            var data = splitMessage[2];
            //If the length of the packet is greater than the length of the data that
            //was received, the packet is not complete and is added to the cache
            if (data.Length < length)
            {
                await AddToCacheAsync(message);
                return;
            }
            
            //The packet is complete, deserialize it and raise the PacketReceived event
            await DeserializePacketAsync(data, prefix);
            //After the complete packet, there may be additional packets in the message, so
            //the packet that was just extracted is removed from the message and the loop continues
            message = data.Substring(length);
        }
    }
    
    /// <summary>
    /// Adds a message to the cache and checks if the cache contains a complete packet
    /// </summary>
    /// <param name="text">The message to add to the cache</param>
    /// <exception cref="InvalidOperationException">Thrown when the cache is not empty and a new packet is being added to it</exception>
    private async Task AddToCacheAsync(string text)
    {
        if(string.IsNullOrEmpty(text)) return;
        if(text.StartsWith('/') && _cache.Length > 0)
            throw new InvalidOperationException("Cache is not empty and a new packet is being added to it.");
        _cache += text;
        var splitMessage = _cache.Split(' ', 3);
        //A packet consists of 3 parts: prefix, length, data
        //If the message does not contain all 3 parts, it is not a complete packet
        if (splitMessage.Length < 3)
            return;
        
        var prefix = splitMessage[0];
        var length = int.Parse(splitMessage[1]);
        var data = splitMessage[2];
        //If the length of the packet is greater than the length of the data that
        //is in the cache, the packet is not complete yet
        if (data.Length < length)
            return;
        
        //The packet is complete, deserialize it and raise the PacketReceived event
        await DeserializePacketAsync(data, prefix);
        //Now that the packet is complete, the cache is cleared
        _cache = string.Empty;
    }

    /// <summary>
    /// Deserializes a packet from a json string and raises the PacketReceived event
    /// </summary>
    /// <param name="json">The json string to deserialize</param>
    /// <param name="prefix">The prefix of the packet in the form /{prefix}</param>
    /// <exception cref="InvalidOperationException">Thrown when the deserialized packet is null</exception>
    private async Task DeserializePacketAsync(string json, string prefix)
    {
        var packet = JsonSerializer.Deserialize(json, PacketBase.GetType(prefix));
        await OnPacketReceivedAsync(packet as PacketBase ?? throw new InvalidOperationException("Packet is null"));
    }

    /// <summary>
    /// Reads a single message from the socket
    /// </summary>
    /// <param name="cancellationToken">For cancelling the reading process</param>
    /// <returns>The message that was read</returns>
    private async Task<string> Read(CancellationToken cancellationToken)
    {
        var buffer = new byte[BufferSize];
        await _socket.ReceiveAsync(buffer, cancellationToken);
        var message = Encoding.ASCII.GetString(buffer).Trim('\0');
        return message;
    }

    protected virtual async Task OnPacketReceivedAsync(PacketBase receivedPacket)
    {
        var task = PacketReceived?.Invoke(receivedPacket);
        if (task != null)
            await task;
    }

    protected virtual void OnError(Exception exception)
    {
        Error?.Invoke(exception);
    }
}