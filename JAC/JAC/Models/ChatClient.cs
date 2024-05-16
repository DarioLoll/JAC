using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using JAC.Shared;
using JAC.Shared.Packets;
using JAC.ViewModels;

namespace JAC.Models;

/// <summary>
/// Represents a client that can connect to a server and send and receive packets (singleton)
/// </summary>
public class ChatClient
{
    private Socket? _socket;
    private SocketReader? _socketReader;
    /// <summary>
    /// The canceller for the listening task (breaking the loop)
    /// </summary>
    private CancellationTokenSource _listeningCanceller = new();
    
    /// <summary>
    /// Whether the client is currently connected to the server
    /// </summary>
    public bool IsConnected => _socket?.Connected ?? false;
    /// <summary>
    /// The singleton instance of the client
    /// </summary>
    public static ChatClient Instance { get; } = new ChatClient();
    /// <summary>
    /// The default IP address to connect to if not specified
    /// </summary>
    public static IPAddress DefaultIpAddress => IPAddress.Loopback;
    /// <summary>
    /// The default port to connect to if not specified
    /// </summary>
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
            _socketReader = new SocketReader(_socket);
            var finalEndPoint = endPoint ?? new IPEndPoint(DefaultIpAddress, DefaultPort);
            await _socket.ConnectAsync(finalEndPoint);
            StartListening();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
    
    /// <summary>
    /// Starts listening for incoming packets asynchronously
    /// </summary>
    private void StartListening()
    {
        _socketReader!.PacketReceived += HandlePacket;
        //fire and forget - the listening task is not awaited (it will run on a separate thread)
        _ = Task.Run(() => _socketReader.ListenAsync(_listeningCanceller.Token));
    }
    
    private async Task HandlePacket(PacketBase packet)
    {
        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            await PacketHandler.HandleAsync(packet);
        });
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
        else OnDisconnected();
    }

    /// <summary>
    /// Shuts down the client and closes the connection
    /// </summary>
    public async Task Close()
    {
        if (IsConnected)
        {
            await Send(new PacketBase(ParameterlessPacket.Disconnect));
            _socket?.Shutdown(SocketShutdown.Both);
            _socket?.Close();
        }
        OnDisconnected();
    }

    private async Task OnLoggedIn(LoginSuccessPacket packet)
    {
        Directory = new ClientDirectory(new User(packet.User));
        await Directory.LoadDataAsync();
        PacketHandler.ChannelsReceived += OnChannelsReceived;
        await Send(new PacketBase(ParameterlessPacket.GetChannels));
    }

    private Task OnChannelsReceived(GetChannelsResponsePacket packet)
    {
        Navigator.Instance.SwitchToViewModel(new MainViewModel(Directory!));
        return Task.CompletedTask;
    }

    protected virtual void OnDisconnected()
    {
        Directory?.SaveDataAsync();
        Disconnected?.Invoke();
    }
}