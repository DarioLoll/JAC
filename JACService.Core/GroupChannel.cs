using System.Text.Json.Serialization;
using JAC.Shared;
using JAC.Shared.Channels;

namespace JACService.Core;

/// <summary>
/// <inheritdoc cref="IGroupChannel"/>
/// </summary>
public class GroupChannel : BaseChannel, IGroupChannel
{
    /// <summary>
    /// <inheritdoc cref="IGroupChannel.Name"/>
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// <inheritdoc cref="IGroupChannel.Description"/>
    /// </summary>
    public string Description { get; set; } = string.Empty;
    /// <summary>
    /// <inheritdoc cref="IGroupChannel.Admins"/>
    /// </summary>
    public IList<string> Admins { get; init; } = new List<string>();
    /// <summary>
    /// <inheritdoc cref="IGroupChannel.Settings"/>
    /// </summary>
    public GroupSettings Settings { get; init; } = new();
    
    /// <summary>
    /// Occurs when the name of the channel is changed.
    /// </summary>
    public event Action<string>? NameChanged;
    /// <summary>
    /// Occurs when the description of the channel is changed.
    /// </summary>
    public event Action<string>? DescriptionChanged;
    /// <summary>
    /// Occurs when the rank of a user in the channel is changed.
    /// </summary>
    public event Action<ChatUser>? RankChanged;

    public GroupChannel(ulong id, DateTime created, string name) : base(id, created)
    {
        Name = name;
    }
    
    [JsonConstructor]
    public GroupChannel(ulong id, DateTime created, string name, string description, IList<ChatUser> chatUsers, 
        IList<Message> messages, IList<string> admins, GroupSettings settings) : base(id, created, chatUsers, messages)
    {
        Name = name;
        Description = description;
        Admins = admins.ToList();
        Settings = settings;
    }
    
    /// <summary>
    /// Adds a user to the channel as a member.
    /// <remarks>Only admins can add users to the group by default</remarks>
    /// </summary>
    /// <param name="user">The user to add to the channel</param>
    /// <param name="adder">The user that is trying to add a user to the channel</param>
    /// <returns>An <see cref="ActionReport"/> indicating whether the user was added successfully, or what error occured</returns>
    public ActionReport AddUser(ChatUser user, ChatUser adder)
    {
        if(Users.Contains(user))
            return ActionReport.Failed(ErrorType.UserAlreadyInChannel);
        bool isAdderInChannel = Users.Contains(adder);
        bool isAdderAdmin = Admins.Contains(adder.Nickname);
        bool areMembersAllowedToAdd = Settings.AllowMembersToAdd;
        if(!isAdderInChannel || (!isAdderAdmin && !areMembersAllowedToAdd))
            return ActionReport.Failed(ErrorType.InsufficientPermissions);
        AddUser(user);
        return ActionReport.SuccessReport;
    }

    /// <summary>
    /// Removes a user from the channel.
    /// <remarks>Only admins can remove users from the group</remarks>
    /// </summary>
    /// <param name="user">The user to remove from the channel</param>
    /// <param name="remover">The user that is trying to remove a user from the channel</param>
    /// <returns>An <see cref="ActionReport"/> indicating whether the user was removed successfully, or what error occured</returns>
    public override ActionReport RemoveUser(ChatUser user, ChatUser remover)
    {
        if(!Users.Contains(user))
            return ActionReport.Failed(ErrorType.UserNotInChannel);
        bool isRemoverAdmin = Admins.Contains(remover.Nickname);
        bool isRemoverInChannel = Users.Contains(remover);
        bool isUserRemover = user == remover;
        if((!isRemoverAdmin || !isRemoverInChannel) && !isUserRemover)
            return ActionReport.Failed(ErrorType.InsufficientPermissions);
        RemoveUser(user);
        return ActionReport.SuccessReport;
    }

    /// <summary>
    /// Sends a message to the channel.
    /// <remarks>Members can be restricted from sending messages by adjusting the <see cref="Settings"/></remarks>
    /// </summary>
    /// <param name="sender">The user that is trying to send a message to the channel</param>
    /// <param name="content">The message content</param>
    /// <returns>An <see cref="ActionReport"/> indicating whether the message was sent successfully, or what error occured</returns>
    public override ActionReport SendMessage(ChatUser sender, string content)
    {
        bool isSenderAdmin = Admins.Contains(sender.Nickname);
        if(Settings.ReadOnlyForMembers && !isSenderAdmin)
            return ActionReport.Failed(ErrorType.InsufficientPermissions);
        return base.SendMessage(sender, content);
    }
    
    /// <summary>
    /// Changes the name of the channel.
    /// <remarks>Only admins can change the group name by default. See <see cref="Settings"/></remarks>
    /// </summary>
    /// <param name="changer">The user that is trying to change the name of the channel</param>
    /// <param name="newName">The new name for the channel</param>
    /// <returns>An <see cref="ActionReport"/> indicating whether the name was changed successfully, or what error occured</returns>
    public ActionReport ChangeName(ChatUser changer, string newName)
    {
        bool isChangerAdmin = Admins.Contains(changer.Nickname);
        bool isChangerInChannel = Users.Contains(changer);
        bool areMembersAllowedToChangeName = Settings.AllowMembersToChangeName;
        if(!isChangerInChannel || (!isChangerAdmin && !areMembersAllowedToChangeName))
            return ActionReport.Failed(ErrorType.InsufficientPermissions);
        Name = newName;
        OnNameChanged(newName);
        return ActionReport.SuccessReport;
    }
    
    /// <summary>
    /// Changes the description of the channel.
    /// <remarks>Everyone can change the description by default. See <see cref="Settings"/></remarks>
    /// </summary>
    /// <param name="changer">The user that is trying to change the description of the channel</param>
    /// <param name="newDescription">The new description for the channel</param>
    /// <returns>An <see cref="ActionReport"/> indicating whether the description was changed successfully, or what error occured</returns>
    public ActionReport ChangeDescription(ChatUser changer, string newDescription)
    {
        bool isChangerAdmin = Admins.Contains(changer.Nickname);
        bool isChangerInChannel = Users.Contains(changer);
        bool areMembersAllowedToChangeDescription = Settings.AllowMembersToChangeDescription;
        if(!isChangerInChannel || (!isChangerAdmin && !areMembersAllowedToChangeDescription))
            return ActionReport.Failed(ErrorType.InsufficientPermissions);
        Description = newDescription;
        OnDescriptionChanged(newDescription);
        return ActionReport.SuccessReport;
    }
    
    /// <summary>
    /// Promotes a user to an admin or demotes an admin to a user.
    /// <remarks>Only admins can change the rank of others.</remarks>
    /// </summary>
    /// <param name="changer">The user that is trying to change the rank of another user</param>
    /// <param name="user">The user whose rank is to be changed</param>
    /// <returns>An <see cref="ActionReport"/> indicating whether the rank was changed successfully, or what error occured</returns>
    public ActionReport ChangeUserRank(ChatUser user, ChatUser changer)
    {
        if(!Users.Contains(user))
            return ActionReport.Failed(ErrorType.UserNotInChannel);
        if(!Admins.Contains(changer.Nickname))
            return ActionReport.Failed(ErrorType.InsufficientPermissions);
        if(Admins.Contains(user.Nickname))
            Admins.Remove(user.Nickname);
        else Admins.Add(user.Nickname);
        OnRankChanged(user);
        return ActionReport.SuccessReport;
    }

    /// <summary>
    /// Creates a NEW group channel, with the creator as the only user and admin.
    /// </summary>
    /// <param name="id">A unique id for the channel. Use <see cref="ChatServiceDirectory.GetNextChannelId"/> in <see cref="ChatServiceDirectory"/></param>
    /// <param name="creator">The user trying to create the group</param>
    /// <param name="name">The name of the group</param>
    /// <param name="description">The description of the group</param>
    /// <returns>An <see cref="ActionResult{T}"/> containing the created group or the error that occured</returns>
    public static ActionResult<GroupChannel> CreateGroupChannel(ulong id, ChatUser creator, string name, 
        string description)
    {
        var channel = new GroupChannel(id, DateTime.Now, name)
        {
            Description = description
        };
        channel.AddUser(creator);
        channel.Admins.Add(creator.Nickname);
        OnChannelCreated(channel);
        return ActionResult<GroupChannel>.Succeeded(channel);
    }

    protected virtual void OnNameChanged(string newName)
    {
        NameChanged?.Invoke(newName);
    }

    protected virtual void OnDescriptionChanged(string obj)
    {
        DescriptionChanged?.Invoke(obj);
    }

    protected virtual void OnRankChanged(ChatUser user)
    {
        RankChanged?.Invoke(user);
    }

    public override string ToString()
    {
        return $"Group {Name}\n\r{base.ToString()}";
    }
}