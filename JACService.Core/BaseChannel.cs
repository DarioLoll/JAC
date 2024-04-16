using System.Text.Json.Serialization;
using JAC.Shared;
using JAC.Shared.Channels;

namespace JACService.Core;

public class BaseChannel : IChannel
{
    public ulong Id { get; }

    [JsonInclude] private List<IUser> _users = new();
    public IEnumerable<IUser> Users => _users;
    
    [JsonInclude] private List<Message> _messages = new();
    public IEnumerable<Message> Messages => _messages;
    
    public IEnumerable<IUser> OnlineUsers => Users.Where(u => u.IsOnline);
    public DateTime Created { get; }

    
    public BaseChannel(ulong id, DateTime created = default)
    {
        Id = id;
        Created = created == default ? DateTime.Now : created;
        OnChannelCreated(this);
    }
    
    public event Action<Message>? MessageSent;

    public static event Action<BaseChannel>? ChannelCreated;
    
    public virtual ActionReport SendMessage(BaseUser sender, string content)
    {
        if(!Users.Contains(sender))
            return new ActionReport{Error = ErrorType.UserNotInChannel};
        var message = new Message(sender, content);
        _messages.Add(message);
        OnMessageSent(message);
        return ActionReport.SuccessReport;
    }
    
    public static ActionResult<BaseChannel> OpenPrivateChannel(ulong id, BaseUser user1, BaseUser user2)
    {
        var channel = new BaseChannel(id);
        channel.AddUser(user1);
        channel.AddUser(user2);
        return ActionResult<BaseChannel>.Succeeded(channel);
    }
    
    protected void AddUser(BaseUser user)
    {
        _users.Add(user);
        user.JoinChannel(Id);
    }
    
    protected void RemoveUser(BaseUser user)
    {
        _users.Remove(user);
        user.LeaveChannel(Id);
    }
    
    public virtual ActionReport RemoveUser(BaseUser user, BaseUser remover)
    {
        if(!Users.Contains(user))
            return ActionReport.Failed(ErrorType.UserNotInChannel);
        if(!Users.Contains(remover))
            return ActionReport.Failed(ErrorType.InsufficientPermissions);
        RemoveUser(user);
        return ActionReport.SuccessReport;
    }

    protected virtual void OnMessageSent(Message sentMessage)
    {
        MessageSent?.Invoke(sentMessage);
    }

    protected static void OnChannelCreated(BaseChannel channel)
    {
        ChannelCreated?.Invoke(channel);
    }
    
}