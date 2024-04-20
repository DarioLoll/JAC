using System.Net;
using System.Net.Sockets;
using JAC.Shared;
using JACService.Core.Contracts;

namespace JACService.Core;

public class Server
{
    private Socket? _socket;
    private ClientManager? _clientManager;

    public static Server Instance { get; } = new Server();
    public IServiceLogger? Logger { get; set; }
    public const ushort DefaultPort = 8080;
    public static IPAddress DefaultIpAddress => IPAddress.Loopback;

    public ushort Port { get; set; } = DefaultPort;

    public IPAddress IpAddress { get; set; } = DefaultIpAddress;

    public bool IsOnline { get; private set; }
    
    public int ClientCount => ClientManager?.Sessions.Count() ?? 0;
    
    public ClientManager? ClientManager => _clientManager;
    

    private Server()
    {
    }

    public bool Start()
    {
        try
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.Bind(new IPEndPoint(IpAddress, Port));
            _socket.Listen(10);
            _clientManager = new ClientManager(_socket, Logger);
            ChatServiceDirectory.Instance.Load();
            _clientManager.AcceptClients();
            IsOnline = true;
            Logger.LogServiceInfo($"Service started on {IpAddress}:{Port}");
            return true;
        }
        catch (Exception e)
        {
            Logger?.LogServiceError(e.Message);
            return false;
        }
    }
    
    public bool Stop()
    {
        if (!IsOnline || _socket == null)
        {
            Logger?.LogServiceError("Service is not running");
            return false;
        }
        try
        {
            ChatServiceDirectory.Instance.Save();
            _socket?.Close();
            IsOnline = false;
            Logger?.LogServiceInfo($"Service stopped on {IpAddress}:{Port}");
            return true;
        }
        catch (Exception e)
        {
            Logger?.LogServiceError(e.Message);
            return false;
        }
    }
    
    public bool Restart()
    {
        Stop();
        return Start();
    }
    
}