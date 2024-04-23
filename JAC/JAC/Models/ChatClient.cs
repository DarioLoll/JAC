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

public class ChatClient
{
    private Socket? _socket;
    public bool IsConnected => _socket?.Connected ?? false;
    public static ChatClient Instance { get; } = new ChatClient();
    public static IPAddress DefaultIpAddress => IPAddress.Loopback;
    public const ushort DefaultPort = 8080;
    public ClientPacketHandler PacketHandler { get; }
    
    public ClientDirectory? Directory { get; set; }
    public event Action? Disconnected;

    private ChatClient()
    {
        PacketHandler = new ClientPacketHandler();
        PacketHandler.LoginSucceeded += OnLoggedIn;
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
    
    public async Task Send(PacketBase packet)
    {
        if (IsConnected)
        {
            await SocketWriter.Send(_socket!, packet);
        }
        OnDisconnected();
    }

    private void Close()
    {
        if (IsConnected)
        {
            _socket?.Shutdown(SocketShutdown.Both);
            _socket?.Close();
        }
        OnDisconnected();
    }

    protected virtual void OnDisconnected()
    {
        Disconnected?.Invoke();
    }
}