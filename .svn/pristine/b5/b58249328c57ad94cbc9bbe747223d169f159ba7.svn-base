using System.Net.Sockets;
using JAC.Shared;
using JAC.Shared.Packets;
using JACService.Core.Contracts;

namespace JACService.Core;

public class Session
{
    private bool _isInSendingState;
    private readonly Socket _socket;
    private readonly SocketReader _socketReader;
    private readonly SocketWriter _socketWriter;
    private readonly PacketHandler _packetHandler;
    public event EventHandler<Session>? SessionClosed;
    public IServiceLogger Logger { get; private set; }

    public IUser? User { get; private set; }

    public Session(Socket socket, IServiceLogger logger)
    {
        _socket = socket;
        Logger = logger;
        _socketReader = new SocketReader(_socket);
        _socketWriter = new SocketWriter(_socket);
        _packetHandler = new PacketHandler()
        {
            PacketHandlers =
            {
                { PacketBase.GetPrefix<LoginPacket>(), HandleLogin }
            }
        };
    }
    
    public async void HandleCommunication()
    {
        try
        {
            while (true)
            {
                var request = await _socketReader.Read();
                string requestType = request.Split(' ', 2)[0].Substring(1);
                Logger.LogRequestInfo($"Received a {requestType} request from {_socket.RemoteEndPoint}");
                _packetHandler.Handle(request);
            }
        }
        catch (Exception)
        {
            await Close();
        }
    }
    public async Task Send(PacketBase packet)
    {
        _isInSendingState = true;
        await _socketWriter.Send(packet);
        _isInSendingState = false;
    }
    
    public async Task SendError(ErrorType errorType) => await Send(new ErrorPacket(errorType));

    public async Task Close()
    {
        if(!_socket.Connected) return;
        //Wait until the socket is done sending data
        await Task.Run(() =>
        {
            while (_isInSendingState) { }
        });
        Logger.LogServiceInfo($"Client disconnected from {_socket.RemoteEndPoint}");
        if (User != null)
            ChatServiceDirectory.Instance.RemoveSession(User);
        _socket.Shutdown(SocketShutdown.Both);
        _socket.Close();
        OnSessionClosed(this);
    }

    protected virtual void OnSessionClosed(Session e)
    {
        SessionClosed?.Invoke(this, e);
    }

    #region Packet Handlers

        public async void HandleLogin(string json)
        {
            LoginPacket? packet = PacketBase.FromJson<LoginPacket>(json);
            if (packet == null)
            {
                await SendError(ErrorType.InvalidPacket);
                return;
            }
            if (User != null)
            {
                await SendError(ErrorType.AlreadyLoggedIn);
                return;
            }
            if(ChatServiceDirectory.Instance.FindUser(packet.Username) != null)
            {
                await SendError(ErrorType.UsernameTaken);
                return;
            }
            User = new ChatUser(packet.Username);
            User.LogIn();
            ChatServiceDirectory.Instance.AddUser(User);
            ChatServiceDirectory.Instance.AddSession(User, this);
            await Send(new LoginSuccessPacket{ Request = packet });
        }
    
    #endregion
}