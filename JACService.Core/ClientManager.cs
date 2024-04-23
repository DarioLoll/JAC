using System.Net.Sockets;
using JAC.Shared;
using JACService.Core.Contracts;

namespace JACService.Core;

public class ClientManager
{
    private readonly Socket _serverSocket;

    private readonly List<Session> _connections = new();
    private readonly Dictionary<BaseUser, Session> _sessions = new();
    
    public IDictionary<BaseUser, Session> Sessions => _sessions;
    public IEnumerable<Session> Connections => _connections;
    public Session? FindSession(BaseUser user) => _sessions.GetValueOrDefault(user);
    
    public Session? FindSession(string nickname)
    {
        var user = ChatServiceDirectory.Instance.FindUser(nickname);
        if (user == null) return null;
        return FindSession(user);
    }

    public void BroadCast(IEnumerable<BaseUser> users, PacketBase packet)
    {
        foreach (var user in users) 
            SendToUser(user, packet);
    }
    
    public void SendToUser(BaseUser user, PacketBase packet)
    {
        var session = FindSession(user);
        session?.Send(packet);
    }
    
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
                _connections.Add(session);
                session.UserLoggedIn += (sender, user) =>
                {
                    _sessions.Add(user, sender);
                    user.IsOnline = true;
                };
                session.SessionClosed += sender =>
                {
                    _sessions.Remove(sender.User!);
                    sender.User!.IsOnline = false;
                    _connections.Remove(sender);
                };

                Thread communicationThread = new(session.HandleCommunication)
                {
                    IsBackground = true
                };
                communicationThread.Start();
            }
        }
        catch (Exception)
        {
            //Assuming that the exception is caused by the server socket being closed
            Logger.LogServiceInfo($"Disconnecting all {_sessions.Count} clients...");
            List<Session> connections = new(_connections);
            foreach (var connection in connections)
                connection.Close();
        }
    }

}