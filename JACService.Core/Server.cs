using System.Net;
using System.Net.Sockets;

namespace MultiprotocolService.Service.Lib;

public class Server
{
    private Socket? _socket;

    private ClientManager? _clientManager;
    
    public IServiceLogger Logger { get; private set; }

    public ushort Port { get; private set; }

    public IPAddress IpAddress { get; private set; } = IPAddress.Any;

    public bool IsOnline { get; private set; }
    
    public int ClientCount => _clientManager?.Sessions.Count() ?? 0;

    public Server(IServiceLogger logger)
    {
        Logger = logger;
    }

    public bool Start(IPAddress ip, ushort port)
    {
        if(port < 1024)
        {
            Logger.LogServiceError("Port number must be greater than 1024");
            return false;
        }
        IpAddress = ip;
        Port = port;
        try
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.Bind(new IPEndPoint(IpAddress, Port));
            _socket.Listen(10);
            _clientManager = new ClientManager(_socket, Logger);
            _clientManager.AcceptClients();
            IsOnline = true;
            Logger.LogServiceInfo($"Service started on {IpAddress}:{Port}");
            return true;
        }
        catch (Exception e)
        {
            Logger.LogServiceError(e.Message);
            return false;
        }
    }
    
    public bool Stop()
    {
        if (!IsOnline || _socket == null)
        {
            Logger.LogServiceError("Service is not running");
            return false;
        }
        try
        {
            _socket?.Close();
            IsOnline = false;
            Logger.LogServiceInfo($"Service stopped on {IpAddress}:{Port}");
            return true;
        }
        catch (Exception e)
        {
            Logger.LogServiceError(e.Message);
            return false;
        }
    }
    
    public bool Restart()
    {
        Stop();
        return Start(IpAddress, Port);
    }
    
}