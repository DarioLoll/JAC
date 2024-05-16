using System.Net.Sockets;
using JAC.Shared;
using JAC.Shared.Packets;
using JACService.Core.Contracts;

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
    public IServiceLogger? Logger { get; }
    
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

    public Session(Socket socket, IServiceLogger? logger)
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
    public void StartListeningAsync()
    {
        _socketReader.PacketReceived += _packetHandler.HandleAsync;
        _socketReader.Error += OnSocketReaderError;
        // _ means that the task is not awaited (runs on another thread)
        _ = Task.Run(() => _socketReader.ListenAsync(_listeningCanceller.Token));
    }

    private void OnSocketReaderError(Exception exception)
    {
        // _ means that the task is not awaited (runs on another thread)
        _ = Task.Run(() => Logger?.LogServiceErrorAsync(exception.Message));
    }

    /// <summary>
    /// Sends a packet to the client on this session
    /// </summary>
    /// <param name="packet">The packet to send</param>
    public async Task Send(PacketBase packet) => await _socketWriter.Send(packet);

    /// <summary>
    /// Sends an error packet to the client on this session
    /// </summary>
    /// <param name="errorType">The type of error to send</param>
    public async Task SendError(ErrorType errorType) => await Send(new ErrorPacket{ErrorType = errorType});

    /// <summary>
    /// Closes the session by sending a disconnect packet and shutting down the socket
    /// </summary>
    public async Task Close()
    {
        if(!_socket.Connected) return;
        if(_isShuttingDown) return;
        
        _isShuttingDown = true;
        await Send(new PacketBase(ParameterlessPacket.Disconnect));
        // Cancel the listening task
        await _listeningCanceller.CancelAsync();
        _socketReader.PacketReceived -= _packetHandler.HandleAsync;
        _socketReader.Error -= OnSocketReaderError;
        Logger?.LogServiceInfoAsync($"Client disconnected from {_socket.RemoteEndPoint}");
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