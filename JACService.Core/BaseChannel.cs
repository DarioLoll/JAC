using System.Text.Json.Serialization;
using JAC.Shared;
using JAC.Shared.Channels;

namespace JACService.Core;

/// <summary>
/// <inheritdoc cref="IChannel"/> from the server's perspective.
/// </summary>
public class BaseChannel : IChannel
{
    /// <summary>
    /// <inheritdoc cref="IChannel.Id"/>
    /// </summary>
    public ulong Id { get; init; }
    /// <summary>
    /// <inheritdoc cref="IChannel.Users"/>
    /// </summary>
    [JsonIgnore] public IList<IUser> Users { get; init; } = new List<IUser>();
    
    [JsonInclude] public IList<ChatUser> ChatUsers => Users.Cast<ChatUser>().ToList();
    
    /// <summary>
    /// <inheritdoc cref="IChannel.Messages"/>
    /// </summary>
    public IList<Message> Messages { get; init; } = new List<Message>();
    /// <summary>
    /// <inheritdoc cref="IChannel.Created"/>
    /// </summary>
    public DateTime Created { get; init; }
    /// <summary>
    /// Returns the users in the channel that are currently online.
    /// </summary>
    [JsonIgnore] public IEnumerable<ChatUser> OnlineUsers => Users.Cast<ChatUser>().Where(user => user.IsOnline);
    
    /// <summary>
    /// Occurs when a message is sent in the channel.
    /// </summary>
    public event Action<Message>? MessageSent;
    /// <summary>
    /// Occurs when a channel is created.
    /// </summary>
    public static event Action<BaseChannel>? ChannelCreated;

    public BaseChannel(ulong id, DateTime created)
    {
        Id = id;
        Created = created;
    }
    
    [JsonConstructor]
    public BaseChannel(ulong id, DateTime created, IList<ChatUser> chatUsers, IList<Message> messages)
    {
        Id = id;
        Created = created;
        Users = chatUsers.Cast<IUser>().ToList();
        Messages = messages.ToList();
    }
    
    public IEnumerable<Message> GetMessages(DateTime since)
    {
        return Messages.Where(message => message.TimeSent > since);
    }
    
    /// <summary>
    /// Sends a message to the channel.
    /// </summary>
    /// <param name="sender">The user that is trying to send a message to the channel</param>
    /// <param name="content">The message content</param>
    /// <returns>An <see cref="ActionReport"/> indicating whether the message was sent successfully, or what error occured</returns>
    public virtual ActionReport SendMessage(ChatUser sender, string content)
    {
        if(!Users.Contains(sender))
            return new ActionReport{Error = ErrorType.UserNotInChannel};
        var message = new Message(sender.Nickname, content);
        Messages.Add(message);
        OnMessageSent(message);
        return ActionReport.SuccessReport;
    }

    /// <summary>
    /// Creates a NEW private channel, with the given users as members.
    /// </summary>
    /// <param name="id">A unique id for the channel. Use <see cref="ChatServiceDirectory.GetNextChannelId"/> in <see cref="ChatServiceDirectory"/></param>
    /// <param name="user1">The first user (whether it is the creator or not, does not matter)</param>
    /// <param name="user2">The second user (whether it is the creator or not, does not matter)</param>
    /// <returns>An <see cref="ActionResult{T}"/> containing the created channel or the error that occured</returns>
    public static ActionResult<BaseChannel> OpenPrivateChannel(ulong id, ChatUser user1, ChatUser user2)
    {
        var channel = new BaseChannel(id, DateTime.Now);
        channel.AddUser(user1);
        channel.AddUser(user2);
        OnChannelCreated(channel);
        return ActionResult<BaseChannel>.Succeeded(channel);
    }
    
    protected void AddUser(ChatUser user)
    {
        Users.Add(user);
        user.JoinChannel(Id);
    }
    
    protected void RemoveUser(ChatUser user)
    {
        Users.Remove(user);
        user.LeaveChannel(Id);
    }
    
    /// <summary>
    /// Removes a user from the channel.
    /// </summary>
    /// <param name="user">The user to remove from the channel</param>
    /// <param name="remover">The user that is trying to remove a user from the channel</param>
    /// <returns>An <see cref="ActionReport"/> indicating whether the user was removed successfully, or what error occured</returns>
    public virtual ActionReport RemoveUser(ChatUser user, ChatUser remover)
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

    public override string ToString()
    {
        if(Id == 0)
            return $"Global Channel: {Users.Count} users.";
        return $"Channel {Id}\n\rCreated at {Created}\n\rBy {Users[0]}\n\rWith {Users.Count} users.";
    }
}