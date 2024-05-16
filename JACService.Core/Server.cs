using System.Net;
using System.Net.Sockets;
using JACService.Core.Contracts;

namespace JACService.Core;

/// <summary>
/// Represents the server (singleton) for the chat that clients connect to
/// </summary>
public class Server
{
    private Socket? _socket;
    private ClientManager? _clientManager;
    /// <summary>
    /// Used to cancel the accepting task (breaking the loop and stopping the <see cref="Socket.Accept"/> method)
    /// </summary>
    private CancellationTokenSource _clientAcceptingCanceller = new();

    /// <summary>
    /// The singleton instance of the server
    /// </summary>
    public static Server Instance { get; } = new();
    
    /// <summary>
    /// The logger service used to log messages
    /// </summary>
    public IServiceLogger? Logger { get; set; }
    
    /// <summary>
    /// The default port that will be used if no port is specified
    /// </summary>
    public const ushort DefaultPort = 8080;
    /// <summary>
    /// The default IP address that will be used if no IP address is specified
    /// </summary>
    public static IPAddress DefaultIpAddress => IPAddress.Loopback;

    /// <summary>
    /// The port that the server will listen on / is listening on
    /// </summary>
    public ushort Port { get; set; } = DefaultPort;

    /// <summary>
    /// The IP address that the server will listen on / is listening on
    /// </summary>
    public IPAddress IpAddress { get; set; } = DefaultIpAddress;

    /// <summary>
    /// Whether the server is online or not
    /// </summary>
    public bool IsOnline { get; private set; }
    
    /// <summary>
    /// The client manager that manages all connected clients
    /// </summary>
    public ClientManager? ClientManager => _clientManager;

    /// <summary>
    /// Occurs when the server is stopping (before it stops)
    /// </summary>
    public event Func<Task>? Stopping;

    private Server()
    {
    }

    /// <summary>
    /// Starts the server and begins accepting clients asynchronously
    /// </summary>
    public async Task StartAsync()
    {
        try
        {
            _clientAcceptingCanceller = new CancellationTokenSource();
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.Bind(new IPEndPoint(IpAddress, Port));
            _socket.Listen(10);
            _clientManager = new ClientManager(_socket, Logger!);
            await ChatServiceDirectory.Instance.LoadAsync(); 
            // fire and forget of the task that accepts clients (runs in the background)
            _ = Task.Run(() => _clientManager.AcceptClients(_clientAcceptingCanceller.Token));
            IsOnline = true;
            await Task.Run(() => Logger?.LogServiceInfoAsync($"Service started on {IpAddress}:{Port}"));
        }
        catch (Exception e)
        {
            await Task.Run(() => Logger?.LogServiceErrorAsync(e.Message));
        }
    }
    
    /// <summary>
    /// Stops the server and disconnects all clients asynchronously
    /// </summary>
    public async Task StopAsync()
    {
        if (!IsOnline || _socket == null) 
            await Task.Run(() => Logger?.LogServiceErrorAsync("Service is not running"));
        try
        {
            await OnStopping();
            // cancel the accepting task
            await _clientAcceptingCanceller.CancelAsync();
            _socket?.Close();
            IsOnline = false;
            await Task.Run(() => Logger?.LogServiceInfoAsync($"Service stopped on {IpAddress}:{Port}"));
            await ChatServiceDirectory.Instance.SaveAsync();
        }
        catch (Exception e)
        {
            await Task.Run(() => Logger?.LogServiceErrorAsync(e.Message));
        }
    }

    protected virtual async Task OnStopping()
    {
        var task = Stopping?.Invoke();
        if (task != null)
            await task;
    }
}