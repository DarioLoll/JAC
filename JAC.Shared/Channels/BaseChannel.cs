using System.Text.Json;

namespace JAC.Shared.Channels;

public class BaseChannel : IChannel
{
    public ulong Id { get; }
    public List<IUser> Users { get; }
    public List<Message> Messages { get; }
    public DateTime Created { get; }

    public BaseChannel(ulong id)
    {
        Id = id;
        Users = new List<IUser>();
        Messages = new List<Message>();
        Created = DateTime.Now;
    }
}