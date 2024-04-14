using JAC.Shared;
using JAC.Shared.Channels;

namespace JACService.Core;

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
        OnChannelCreated(this);
    }
    
    public event Action<Message>? MessageSent;
    public static event Action<BaseChannel>? ChannelCreated;
    
    public virtual ActionReport SendMessage(IUser sender, string content)
    {
        if(!Users.Contains(sender))
            return new ActionReport{Error = ErrorType.UserNotInChannel};
        var message = new Message(sender, content);
        Messages.Add(message);
        OnMessageSent(message);
        return ActionReport.SuccessReport;
    }
    
    public static ActionResult<BaseChannel> OpenPrivateChannel(ulong id, IUser user1, IUser user2)
    {
        var channel = new BaseChannel(id);
        channel.AddUser(user1);
        channel.AddUser(user2);
        return ActionResult<BaseChannel>.Succeeded(channel);
    }
    
    protected void AddUser(IUser user)
    {
        Users.Add(user);
        user.Channels.Add(Id);
    }
    
    protected void RemoveUser(IUser user)
    {
        Users.Remove(user);
        user.Channels.Remove(Id);
    }
    
    public virtual ActionReport RemoveUser(IUser user, IUser remover)
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