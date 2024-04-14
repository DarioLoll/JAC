using JAC.Shared;
using JAC.Shared.Channels;

namespace JACService.Core;

public class ChatServiceDirectory
{
    public static ChatServiceDirectory Instance { get; } = new ChatServiceDirectory();
    private ChatServiceDirectory() { }

    private readonly List<IUser> _users = new();


    private Random _random = new();
    public Random Random => _random;

    internal List<BaseChannel> Channels { get; } = new();


    public void AddUser(IUser user) => _users.Add(user);

    public void RemoveUser(IUser user) => _users.Remove(user);

    public IUser? FindUser(string nickname) => _users.Find(user => user.Nickname == nickname);
    
    public BaseChannel? GetChannel(ulong id) => Channels.Find(channel => channel.Id == id);

    public static IEnumerable<IChannel> GetChannels(IUser user) => Instance.Channels.Where(channel => user.Channels.Contains(channel.Id));
    
    /// <summary>
    /// Generates a new unique channel id. Channels are identified by their unique id.
    /// </summary>
    /// <returns>A random ulong that isn't already taken by a channel in the <see cref="JACService.Core.ChatServiceDirectory"/></returns>
    public ulong GetNextChannelId()
    {
        ulong id = (ulong)Random.Next() * (ulong)Random.Next();
        bool channelWithIdExists = Channels.Exists(channel => channel.Id == id);
        while (channelWithIdExists)
        {
            id = (ulong)Random.Next() * (ulong)Random.Next();
            channelWithIdExists = Channels.Exists(channel => channel.Id == id);
        }
        return id;
    }
}