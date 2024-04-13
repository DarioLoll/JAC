using JAC.Shared;
using JAC.Shared.Channels;
using JACService.Core.Contracts;

namespace JACService.Core;

public class ChatServiceDirectory
{
    public static ChatServiceDirectory Instance { get; } = new ChatServiceDirectory();
    private ChatServiceDirectory() { }

    private readonly List<IUser> _users = new();


    private Random _random = new();
    public Random Random => _random;

    internal List<IChannel> Channels { get; } = new();


    public void AddUser(IUser user) => _users.Add(user);

    public void RemoveUser(IUser user) => _users.Remove(user);

    public IUser? FindUser(string nickname) => _users.Find(user => user.Nickname == nickname);
    
    public IChannel? GetChannel(ulong id) => Channels.Find(channel => channel.Id == id);

    public static IEnumerable<IChannel> GetChannels(IUser user) => Instance.Channels.Where(channel => user.Channels.Contains(channel.Id));
}