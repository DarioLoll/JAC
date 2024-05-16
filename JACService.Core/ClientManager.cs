using System.Net.Sockets;
using JAC.Shared;
using JACService.Core.Contracts;

namespace JACService.Core;

/// <summary>
/// Manages all connected clients and provides methods to send packets to them
/// </summary>
public class ClientManager
{
    private readonly Socket _serverSocket;

    private readonly List<Session> _connections = new();
    private readonly Dictionary<ChatUser, Session> _sessions = new();
    
    /// <summary>
    /// The logger service used to log messages
    /// </summary>
    public IServiceLogger? Logger { get; }
    
    /// <summary>
    /// Finds the session associated with a user
    /// </summary>
    /// <returns>A session on which the given user is logged in</returns>
    public Session? FindSession(ChatUser user) => _sessions.GetValueOrDefault(user);

    /// <summary>
    /// Sends the given packet to all users in the given collection
    /// </summary>
    public void BroadCast(IEnumerable<ChatUser> users, PacketBase packet)
    {
        foreach (var user in users) 
            SendToUser(user, packet);
    }
    
    /// <summary>
    /// Sends the given packet to the given user
    /// </summary>
    public void SendToUser(ChatUser user, PacketBase packet)
    {
        var session = FindSession(user);
        session?.Send(packet);
    }
    
    public ClientManager(Socket serverSocket, IServiceLogger? logger)
    {
        _serverSocket = serverSocket;
        Logger = logger;
        Server.Instance.Stopping += OnServerStopping;
    }

    /// <summary>
    /// Closes all client sessions when the server is stopping
    /// </summary>
    private async Task OnServerStopping()
    {
        Logger?.LogServiceInfoAsync($"Disconnecting all {_sessions.Count} clients...");
        List<Session> connections = new(_connections);
        foreach (var connection in connections)
           await connection.Close();
    }

    /// <summary>
    /// Accepts clients in a loop and creates a session for each client
    /// </summary>
    /// <param name="cancellationToken">The cancellation token that will be used to cancel the accepting task</param>
    public async Task AcceptClients(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var clientSocket = await _serverSocket.AcceptAsync(cancellationToken);
                await OnClientAccepted(clientSocket);
            }
            catch(OperationCanceledException)
            {
                break;
            }
            catch (Exception e)
            {
                await Task.Run(() => Logger?.LogServiceErrorAsync(e.Message));
                break;
            }
        }
    }

    /// <summary>
    /// Creates a session for the given client socket and starts listening for incoming packets
    /// </summary>
    /// <param name="clientSocket">The socket of the client that connected</param>
    private async Task OnClientAccepted(Socket clientSocket)
    {
        await Task.Run(() => 
            Logger?.LogServiceInfoAsync($"Client connected from {clientSocket.RemoteEndPoint}"));
        var session = new Session(clientSocket, Logger);
        _connections.Add(session);
        session.UserLoggedIn += OnUserLoggedIn;
        session.SessionClosed += OnSessionClosed;
        session.StartListeningAsync();
    }
    
    private void OnUserLoggedIn(Session session, ChatUser user)
    {
        _sessions.Add(user, session);
        user.IsOnline = true;
    }
    
    private void OnSessionClosed(Session session)
    {
        _connections.Remove(session);
        if(session.User == null) return;
        _sessions.Remove(session.User!);
        session.User!.IsOnline = false;
        session.User.LastSeen = DateTime.Now;
    }

}