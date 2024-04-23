using System;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using Avalonia.Threading;
using JAC.Shared;
using JAC.Shared.Channels;
using JAC.Shared.Packets;

namespace JAC.Models;

/// <summary>
/// Represents a client that can connect to a server and send and receive packets (singleton)
/// </summary>
public class ChatClient
{
    private Socket? _socket;
    public bool IsConnected => _socket?.Connected ?? false;
    public static ChatClient Instance { get; } = new ChatClient();
    public static IPAddress DefaultIpAddress => IPAddress.Loopback;
    public const ushort DefaultPort = 8080;
    
    /// <summary>
    /// <inheritdoc cref="ClientPacketHandler"/>
    /// </summary>
    public ClientPacketHandler PacketHandler { get; }
    /// <summary>
    /// <inheritdoc cref="ClientDirectory"/>
    /// </summary>
    public ClientDirectory? Directory { get; set; }
    /// <summary>
    /// Occurs when the client has been disconnected from the server
    /// </summary>
    public event Action? Disconnected;

    private ChatClient()
    {
        PacketHandler = new ClientPacketHandler();
        PacketHandler.LoginSucceeded += OnLoggedIn;
    }
    
    /// <summary>
    /// Connects to a server at the specified endpoint (or the default endpoint if not specified)
    /// </summary>
    /// <returns>Whether the connection was successful</returns>
    public async Task<bool> Connect(IPEndPoint? endPoint = default)
    {
        if (_socket != null && _socket.Connected) return true;
        try
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var finalEndPoint = endPoint ?? new IPEndPoint(DefaultIpAddress, DefaultPort);
            await _socket.ConnectAsync(finalEndPoint);
            Listen();
            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }
    
    /// <summary>
    /// Starts listening for incoming packets asynchronously
    /// </summary>
    private async void Listen()
    {
        try
        {
            while (IsConnected)
            {
                var request = await SocketReader.Read(_socket!);
                Dispatcher.UIThread.Invoke(() => PacketHandler.Handle(request));
            }
            Close();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Exception occured: {e.Message}");
            Close();
        }
    }
    
    /// <summary>
    /// Sends a packet to the server
    /// <remarks>Trying to send a packet when not connected won't throw an exception</remarks>
    /// </summary>
    /// <param name="packet">The packet to send to the server</param>
    public async Task Send(PacketBase packet)
    {
        if (IsConnected)
        {
            await SocketWriter.Send(_socket!, packet);
        }
        OnDisconnected();
    }

    /// <summary>
    /// Shuts down the client and closes the connection
    /// </summary>
    private void Close()
    {
        if (IsConnected)
        {
            _socket?.Shutdown(SocketShutdown.Both);
            _socket?.Close();
        }
        OnDisconnected();
    }

    private async void OnLoggedIn(LoginSuccessPacket packet)
    {
        Directory = new ClientDirectory {User = new User(packet.User)};
        PacketHandler.ChannelsReceived += OnChannelsReceived;
        await Send(new PacketBase(ParameterlessPacket.GetChannels));
    }

    private void OnChannelsReceived(GetChannelsResponsePacket packet)
    {
        foreach (var channelModel in packet.Channels)
        {
            if (channelModel is IGroupChannel groupChannel)
            {
                Directory!.AddChannel(new GroupChannel(groupChannel));
            }
            else
            {
                Directory!.AddChannel(new BaseChannel(channelModel));
            }
        }
    }

    protected virtual void OnDisconnected()
    {
        Disconnected?.Invoke();
    }
}