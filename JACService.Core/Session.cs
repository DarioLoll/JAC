using System.Net.Sockets;
using JAC.Shared;
using JAC.Shared.Packets;
using JACService.Core.Contracts;

namespace JACService.Core;

public class Session
{
    private readonly Socket _socket;
    private readonly SocketReader _socketReader;
    private readonly SocketWriter _socketWriter;
    private readonly PacketHandler _packetHandler;
    public event Action<Session>? SessionClosed;
    public event Action<Session, ChatUser>? UserLoggedIn;
    public IServiceLogger Logger { get; private set; }

    private ChatUser? _user;
    
    private bool _isShuttingDown = false;

    public ChatUser? User
    {
        get => _user;
        set
        {
            _user = value;
            if(value == null) return;
            OnUserLoggedIn(value);
        }
    }

    public Session(Socket socket, IServiceLogger logger)
    {
        _socket = socket;
        Logger = logger;
        _socketReader = new SocketReader(_socket);
        _socketWriter = new SocketWriter(_socket);
        _packetHandler = new ServerPacketHandler(this);
    }
    
    public async void HandleCommunication()
    {
        try
        {
            while (true)
            {
                var request = await _socketReader.Read();
                if (string.IsNullOrEmpty(request)) continue;
                string requestType = request.Split(' ', 2)[0].Substring(1);
                Logger.LogRequestInfo($"Received a {requestType} request from {_socket.RemoteEndPoint}");
                _packetHandler.Handle(request);
            }
        }
        catch (Exception e)
        {
            await Close();
        }
    }
    public async Task Send(PacketBase packet) => await _socketWriter.Send(packet);

    public async Task SendError(ErrorType errorType) => await Send(new ErrorPacket{ErrorType = errorType});

    public async Task Close()
    {
        if(!_socket.Connected) return;
        if(_isShuttingDown) return;
        _isShuttingDown = true;
        await Send(new PacketBase(ParameterlessPacket.Disconnect));
        Logger.LogServiceInfo($"Client disconnected from {_socket.RemoteEndPoint}");
        _socket.Shutdown(SocketShutdown.Both);
        _socket.Close();
        OnSessionClosed();
    }

    protected virtual void OnSessionClosed()
    {
        SessionClosed?.Invoke(this);
    }


    protected virtual void OnUserLoggedIn(ChatUser user)
    {
        UserLoggedIn?.Invoke(this, user);
    }
}