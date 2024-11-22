using System.Net.Sockets;
using JAC.Shared;
using JAC.Shared.Packets;
using JACService.Core.Logging;

namespace JACService.Core;

/// <summary>
/// Represents a session between the server and a single client
/// </summary>
public class Session
{
    private readonly Socket _socket;
    private readonly SocketReader _socketReader;
    private readonly SocketWriter _socketWriter;
    private readonly PacketHandler _packetHandler;
    
    /// <summary>
    /// Used to avoid calling <see cref="Close"/> multiple times
    /// </summary>
    private bool _isShuttingDown;

    /// <summary>
    /// Used to cancel the listening task (breaking the loop and stopping the Socket.Receive method)
    /// </summary>
    private readonly CancellationTokenSource _listeningCanceller = new();
    
    /// <summary>
    /// The logger service used to log messages
    /// </summary>
    public IServiceLogger Logger { get; }
    
    private ChatUser? _user;
    /// <summary>
    /// The user that is logged in on this session
    /// </summary>
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
    
    /// <summary>
    /// Occurs when this session is closed (the client disconnects)
    /// </summary>
    public event Action<Session>? SessionClosed;
    
    /// <summary>
    /// Occurs when the client on this session logs in
    /// </summary>
    public event Action<Session, ChatUser>? UserLoggedIn;

    public Session(Socket socket, IServiceLogger logger)
    {
        _socket = socket;
        Logger = logger;
        _socketReader = new SocketReader(_socket);
        _socketWriter = new SocketWriter(_socket);
        _packetHandler = new ServerPacketHandler(this);
    }
    
    /// <summary>
    /// Starts listening for incoming packets from the client ("fire and forget" of <see cref="SocketReader.ListenAsync"/>)
    /// </summary>
    public void StartListening()
    {
        _socketReader.PacketReceived += HandlePacket;
        _socketReader.Error += OnSocketReaderError;
        // _ means that the task is not awaited (runs on another thread)
        _ = Task.Run(() => _socketReader.ListenAsync(_listeningCanceller.Token));
    }

    private void HandlePacket(PacketBase packet)
    {
        Logger.LogAsync(LogType.Request,
            $"Received a {packet.GetPrefix()} packet from {User?.Nickname ?? $"{_socket.RemoteEndPoint}"}.");
        _packetHandler.Handle(packet);
    }

    private async Task OnSocketReaderError(Exception exception)
    {
        await Logger.LogAsync(LogType.Error, "An error occurred while reading from the socket.");
        await Logger.LogAsync(LogType.Error, exception.Message, true);
    }

    /// <summary>
    /// Sends a packet to the client on this session
    /// </summary>
    /// <param name="packet">The packet to send</param>
    public async Task Send(PacketBase packet)
    {
        if(User == null && packet.GetPrefix() != PacketBase.GetPrefix<ErrorPacket>())
        {
            await Logger.LogAsync(LogType.Warning, $"Attempted to send a ({packet.GetPrefix()}) packet to a null user.", true);
            return;
        }
        await Logger.LogAsync(LogType.Info,$"Sending a {packet.GetPrefix()} packet to {User!.Nickname}",true);
        await _socketWriter.Send(packet);
    }

    /// <summary>
    /// Sends an error packet to the client on this session
    /// </summary>
    /// <param name="errorType">The type of error to send</param>
    public async Task SendError(ErrorType errorType) => await Send(new ErrorPacket{ErrorType = errorType});

    /// <summary>
    /// Closes the session by sending a disconnect packet and shutting down the socket
    /// </summary>
    public async Task Close(bool clientAlreadyDisconnected)
    {
        if(!_socket.Connected) return;
        if(_isShuttingDown) return;
        
        _isShuttingDown = true;
        await Logger.LogAsync(LogType.Info, $"Closing the session {_socket.RemoteEndPoint} ({User?.Nickname ?? "No User"}).", true);
        if(!clientAlreadyDisconnected)
            await Send(new PacketBase(ParameterlessPacket.Disconnect));
        await Logger.LogAsync(LogType.Info, $"Stopping the listening.", true);
        await _listeningCanceller.CancelAsync();
        await Logger.LogAsync(LogType.Info, $"Stopped listening.", true);
        _socketReader.PacketReceived -= HandlePacket;
        _socketReader.Error -= OnSocketReaderError;
        if (_socket.Connected)
            _socket.Shutdown(SocketShutdown.Both);
        _socket.Close();
        await Logger.LogAsync(LogType.Info,$"Client disconnected from ({User?.Nickname ?? "No User"}).");
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