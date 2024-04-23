using JAC.Shared;
using JAC.Shared.Channels;

namespace JACService.Core;

public class BaseChannel : IChannel
{
    public required ulong Id { get; init; }
    public IList<IUser> Users { get; init; } = new List<IUser>();
    public IList<Message> Messages { get; init; } = new List<Message>();
    public required DateTime Created { get; init; }
    
    public IEnumerable<BaseUser> OnlineUsers => Users.Cast<BaseUser>().Where(user => user.IsOnline);
    
    public event Action<Message>? MessageSent;

    public static event Action<BaseChannel>? ChannelCreated;
    
    public virtual ActionReport SendMessage(BaseUser sender, string content)
    {
        if(!Users.Contains(sender))
            return new ActionReport{Error = ErrorType.UserNotInChannel};
        var message = new Message(sender, content);
        Messages.Add(message);
        OnMessageSent(message);
        return ActionReport.SuccessReport;
    }
    
    public static ActionResult<BaseChannel> OpenPrivateChannel(ulong id, BaseUser user1, BaseUser user2)
    {
        var channel = new BaseChannel
        {
            Id = id,
            Created = DateTime.Now
        };
        channel.AddUser(user1);
        channel.AddUser(user2);
        OnChannelCreated(channel);
        return ActionResult<BaseChannel>.Succeeded(channel);
    }
    
    public static BaseChannel CreateFromModel(IChannel channelModel)
    {
        return new BaseChannel
        {
            Id = channelModel.Id,
            Users = BaseUser.CreateFromModels(channelModel.Users),
            Messages = channelModel.Messages.ToList(),
            Created = channelModel.Created
        };
    }
    
    protected void AddUser(BaseUser user)
    {
        Users.Add(user);
        user.JoinChannel(Id);
    }
    
    protected void RemoveUser(BaseUser user)
    {
        Users.Remove(user);
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