using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Avalonia.Threading;
using JAC.Shared;
using JAC.Shared.Packets;

namespace JAC.Models;

public class ChatClient
{
    private Socket? _socket;
    public bool IsConnected => _socket?.Connected ?? false;
    public static ChatClient Instance { get; } = new ChatClient();
    public static IPAddress DefaultIpAddress => IPAddress.Loopback;
    public const ushort DefaultPort = 8080;
    private PacketHandler _packetHandler;

    public event Action<LoginSuccessPacket>? LoginSuccess;
    public event Action<ErrorPacket>? Error;
    public event Action? Disconnected;

    private ChatClient()
    {
        _packetHandler = new PacketHandler()
        {
            PacketHandlers =
            {
                { PacketBase.GetPrefix<LoginSuccessPacket>(), HandleLoginSuccess },
                { PacketBase.GetPrefix<ErrorPacket>(), HandleError }
            }
        };
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
                Dispatcher.UIThread.Invoke(() => _packetHandler.Handle(request));
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

    #region Packet Handlers

        private void HandleLoginSuccess(string json)
        {
            LoginSuccessPacket? packet = PacketBase.FromJson<LoginSuccessPacket>(json);
            if (packet != null) OnLoginSuccess(packet);
        }
        
        private void HandleError(string json)
        {
            ErrorPacket? packet = PacketBase.FromJson<ErrorPacket>(json);
            if (packet != null) OnError(packet);
        }

    #endregion

    protected virtual void OnLoginSuccess(LoginSuccessPacket packet)
    {
        LoginSuccess?.Invoke(packet);
    }

    protected virtual void OnDisconnected()
    {
        Disconnected?.Invoke();
    }

    protected virtual void OnError(ErrorPacket packet)
    {
        Error?.Invoke(packet);
    }
}