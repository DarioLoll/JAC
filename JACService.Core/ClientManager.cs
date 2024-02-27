using System.Net.Sockets;

namespace MultiprotocolService.Service.Lib;

public class ClientManager
{
    private readonly Socket _serverSocket;

    private readonly List<Session> _sessions = new();

    public IEnumerable<Session> Sessions => _sessions;
    
    public IServiceLogger Logger { get; private set; }
    
    public ClientManager(Socket serverSocket, IServiceLogger logger)
    {
        _serverSocket = serverSocket;
        Logger = logger;
    }
    
    public async void AcceptClients()
    {
        try
        {
            while (true)
            {
                var clientSocket = await _serverSocket.AcceptAsync();
                Logger.LogServiceInfo($"Client connected from {clientSocket.RemoteEndPoint}");
                var session = new Session(clientSocket, Logger);
                session.SessionClosed += (_, closedSession) => _sessions.Remove(closedSession);
                _sessions.Add(session);
                session.HandleCommunication();
            }
        }
        catch (Exception)
        {
            //Assuming that the exception is caused by the server socket being closed
            Logger.LogServiceInfo($"Disconnecting all {_sessions.Count} clients...");
            List<Session> sessions = new(_sessions);
            foreach (var session in sessions) await session.Close();
            _sessions.Clear();
        }
    }
}