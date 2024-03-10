using JACService.Core.Contracts;

namespace JACService.Core;

public class ChatServiceDirectory
{
    public static ChatServiceDirectory Instance { get; } = new ChatServiceDirectory();
    private ChatServiceDirectory() { }

    private readonly List<IUser> _users = new();

    private readonly Dictionary<IUser, Session> _sessions = new();

    public void AddUser(IUser user) => _users.Add(user);

    public void RemoveUser(IUser user) => _users.Remove(user);

    public void AddSession(IUser user, Session session) => _sessions.Add(user, session);

    public void RemoveSession(IUser user) => _sessions.Remove(user);

    public IUser? FindUser(string nickname) => _users.Find(user => user.Nickname == nickname);
    
    public Session? FindSession(IUser user) => _sessions.GetValueOrDefault(user);

    public Session? FindSession(string nickname)
    {
        IUser? user = FindUser(nickname);
        if (user == null) return null;
        return FindSession(user);
    }
    
}