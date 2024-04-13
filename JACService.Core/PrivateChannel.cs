using JAC.Shared;
using JAC.Shared.Channels;

namespace JACService.Core;

public class PrivateChannel : IChannel
{
    public ulong Id { get; }
    public List<IUser> Users { get; }
    public List<Message> Messages { get; }
    public DateTime Created { get; }
    
    public PrivateChannel(ulong id, IUser user1, IUser user2)
    {
        Id = id;
        Users = new List<IUser> { user1, user2 };
        Messages = new List<Message>();
        Created = DateTime.Now;
    }
}