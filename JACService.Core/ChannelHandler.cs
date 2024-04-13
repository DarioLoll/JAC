using JAC.Shared;
using JAC.Shared.Channels;
using JAC.Shared.Packets;

namespace JACService.Core;

/// <summary>
/// Handles the logic for channels and channel members on the server.
/// </summary>
public static class ChannelHandler
{
    /// <summary>
    /// Event that is triggered when a member changes in a channel (joins, leaves or their ranks changes).
    /// </summary>
    public static event Action<IChannel, IUser, ChannelMemberChangeType>? MemberChanged;
    
    /// <summary>
    /// Event that is triggered when a message is sent in a channel.
    /// </summary>
    public static event Action<IChannel, Message>? MessageSent;
    
    /// <summary>
    /// Event that is triggered when the name of a group channel is changed.
    /// </summary>
    public static event Action<GroupChannel, string>? GroupNameChanged;
    
    /// <summary>
    /// Event that is triggered when the description of a group channel is changed.
    /// </summary>
    public static event Action<GroupChannel, string>? GroupDescriptionChanged;
    
    /// <summary>
    /// Event that is triggered when a new channel is created.
    /// </summary>
    public static event Action<IChannel>? ChannelCreated;
    
    /// <returns>
    /// <list type="bullet">
    /// <item>Success: The message was sent successfully.</item>
    /// <item>ChannelNotFound: The channel was not found (null).</item>
    /// <item>UserNotFound: The sender was not found (null).</item>
    /// <item>UserNotInChannel: The sender is not in the channel.</item>
    /// </list>
    /// </returns>
    public static ActionResult SendMessage(this IChannel? channel, IUser? sender, string message)
    {
        if (channel == null) return ActionResult.ChannelNotFound;
        if (sender == null) return ActionResult.UserNotFound;
        if (!channel.Users.Contains(sender)) return new ActionResult { Error = ErrorType.UserNotInChannel };
        Message newMessage = new(sender, message);
        channel.Messages.Add(newMessage);
        OnMessageSent(channel, newMessage);
        return ActionResult.SuccessResult;
    }

    private static bool IsUserMember(this IChannel channel, IUser user) => channel.Users.Contains(user);
    private static bool IsUserAdmin(this GroupChannel channel, IUser user) => channel.Admins.Contains(user);

    /// <summary>
    /// Checks if the given user has the permission to change the rank of other users in the channel.
    /// </summary>
    /// <param name="channel">The channel to check the given user's permission in</param>
    /// <param name="user">The user to check the permission of</param>
    /// <returns>
    /// <c>true</c> if the user has permission.
    /// <c>false</c> if the user does not have permission.
    /// </returns>
    public static bool CheckChangeUserRankPermissions(this GroupChannel channel, IUser user)
    {
        return channel.IsUserMember(user) && channel.IsUserAdmin(user);
    }

    /// <summary>
    /// Checks if the given user has the permission to change the name and description of the group channel.
    /// </summary>
    /// <param name="channel">The channel to check the given user's permission in</param>
    /// <param name="user">The user to check the permission of</param>
    /// <returns>
    /// <c>true</c> if the user has permission.
    /// <c>false</c> if the user does not have permission.
    /// </returns>
    public static bool CheckChangeGroupDetailsPermissions(this GroupChannel channel, IUser user)
    {
        return channel.IsUserMember(user) && (channel.Settings.AllowMembersToEdit || channel.IsUserAdmin(user));
    }

    /// <summary>
    /// Checks if the given user has the permission to add other users to the group channel.
    /// </summary>
    /// <param name="channel">The channel to check the given user's permission in</param>
    /// <param name="user">The user to check the permission of</param>
    /// <returns>
    /// <c>true</c> if the user has permission.
    /// <c>false</c> if the user does not have permission.
    /// </returns>
    public static bool CheckAddUserPermissions(this GroupChannel channel, IUser user)
    {
        return channel.IsUserMember(user) && (channel.Settings.AllowMembersToAdd || channel.IsUserAdmin(user));
    }
    
    /// <summary>
    /// Creates a new group channel with the given user as the creator and only member.
    /// </summary>
    /// <param name="asUser">The user that is creating the group</param>
    /// <param name="name">The name of the group</param>
    /// <param name="description">The description of the group</param>
    public static void CreateGroup(IUser asUser, string name, string description = "")
    {
        GroupChannel channel = new(GetNextChannelId(), asUser, name, description);
        ChatServiceDirectory.Instance.Channels.Add(channel);
        OnChannelCreated(channel);
        OnMemberChanged(channel, asUser, ChannelMemberChangeType.Joined);
    }
    
    /// <summary>
    /// Creates a new private channel (chat/conversation) between the two given users.
    /// </summary>
    /// <returns>If the action was successful or not, and the error that occured if it was not successful</returns>
    public static ActionResult OpenPrivateChannel(IUser? user1, IUser? user2)
    {
        if(user1 == null || user2 == null) return ActionResult.UserNotFound;
        bool channelWithUsersExists = ChatServiceDirectory.Instance.Channels.Exists(channel =>
            channel.Users.Contains(user1) && channel.Users.Contains(user2));
        if (channelWithUsersExists) 
            return new ActionResult { Error = ErrorType.ChannelAlreadyExists };
        PrivateChannel channel = new(GetNextChannelId(), user1, user2);
        ChatServiceDirectory.Instance.Channels.Add(channel);
        OnChannelCreated(channel);
        OnMemberChanged(channel, user1, ChannelMemberChangeType.Joined);
        OnMemberChanged(channel, user2, ChannelMemberChangeType.Joined);
        return ActionResult.SuccessResult;
    }
    
    /// <summary>
    /// Generates a new unique channel id. Channels are identified by their unique id.
    /// </summary>
    /// <returns>A random ulong that isn't already taken by a channel in the <see cref="JACService.Core.ChatServiceDirectory"/></returns>
    private static ulong GetNextChannelId()
    {
        Random random = ChatServiceDirectory.Instance.Random;
        ulong id = (ulong)random.Next() * (ulong)random.Next();
        bool channelWithIdExists = ChatServiceDirectory.Instance.Channels.Exists(channel => channel.Id == id);
        while (channelWithIdExists)
        {
            id = (ulong)random.Next() * (ulong)random.Next();
            channelWithIdExists = ChatServiceDirectory.Instance.Channels.Exists(channel => channel.Id == id);
        }
        return id;
    }

    /// <summary>
    /// Sets the name of the group channel.
    /// </summary>
    /// <param name="name">The new name of the channel</param>
    /// <param name="requestedBy">The user that wants to change the name of the group</param>
    /// <param name="channel">The channel to change the name of</param>
    /// <returns>If the action was successful or not, and the error that occured if it was not successful</returns>
    public static ActionResult SetGroupName(this GroupChannel? channel, string name, IUser? requestedBy)
    {
        if (channel == null) return ActionResult.ChannelNotFound;
        if (requestedBy == null) return ActionResult.UserNotFound;
        if(CheckChangeGroupDetailsPermissions(channel, requestedBy)) 
            return new ActionResult { Error = ErrorType.InsufficientPermissions };
        channel.Name = name;
        OnGroupNameChanged(channel, name);
        return ActionResult.SuccessResult;
    }

    /// <summary>
    /// Sets the description of the group channel.
    /// </summary>
    /// <param name="description">The new description of the channel</param>
    /// <param name="requestedBy">The user that wants to change the description of the group</param>
    /// <param name="channel">The channel to change the description of</param>
    /// <returns>If the action was successful or not, and the error that occured if it was not successful</returns>
    public static ActionResult SetDescription(this GroupChannel? channel, string description, IUser? requestedBy)
    {
        if (channel == null) 
            return ActionResult.ChannelNotFound;
        if (requestedBy == null) 
            return ActionResult.UserNotFound;
        if(CheckChangeGroupDetailsPermissions(channel, requestedBy)) 
            return new ActionResult { Error = ErrorType.InsufficientPermissions };
        channel.Description = description;
        OnGroupDescriptionChanged(channel, description);
        return ActionResult.SuccessResult;
    }

    /// <summary>
    /// Adds the given user to the group channel as a member.
    /// </summary>
    /// <param name="user">The user to add to the given channel as a member</param>
    /// <param name="addedBy">The user that wants to add the given user to the group</param>
    /// <param name="channel">The channel to add the user to</param>
    /// <returns></returns>
    public static ActionResult AddUser(this GroupChannel? channel, IUser? user, IUser addedBy)
    {
        if (channel == null) return ActionResult.ChannelNotFound;
        if (user == null) return ActionResult.UserNotFound;
        if (channel.Users.Contains(user))
            return new ActionResult { Error = ErrorType.UserAlreadyInChannel };
        if (user == addedBy)
            return new ActionResult { Error = ErrorType.CannotAddSelf };
        if (!channel.CheckAddUserPermissions(addedBy))
            return new ActionResult { Error = ErrorType.InsufficientPermissions };
        channel.Users.Add(user);
        user.Channels.Add(channel.Id);
        OnMemberChanged(channel, user, ChannelMemberChangeType.Joined);
        return ActionResult.SuccessResult;
    }

    /// <summary>
    /// Promotes the given user to an admin if they are not already an admin, or demotes them if they are.
    /// </summary>
    /// <param name="channel">The channel to change the rank of the given user in</param>
    /// <param name="user">The user to change the rank of</param>
    /// <param name="requestedBy">The user that wants to promote/demote the other user</param>
    /// <returns>If the action was successful or not, and the error that occured if it was not successful</returns>
    public static ActionResult ChangeUserRank(this GroupChannel? channel, IUser? user, IUser requestedBy)
    {
        if (channel == null)
            return ActionResult.ChannelNotFound;
        if (user == null)
            return ActionResult.UserNotFound;
        if (!channel.IsUserMember(user))
            return new ActionResult { Error = ErrorType.UserNotInChannel };
        if (!channel.CheckChangeUserRankPermissions(requestedBy))
            return new ActionResult { Error = ErrorType.InsufficientPermissions };
        if (channel.Admins.Contains(user))
            channel.Admins.Remove(user);
        else channel.Admins.Add(user);
        OnMemberChanged(channel, user, ChannelMemberChangeType.RankChanged);
        return ActionResult.SuccessResult;
    }

    /// <summary>
    /// Kicks the given user out of the given channel
    /// </summary>
    /// <param name="channel">The channel to kick the given user from</param>
    /// <param name="user">The user to kick out of the given channel</param>
    /// <param name="removedBy">The user that wants to kick the other user</param>
    /// <returns>If the action was successful or not, and the error that occured if it was not successful</returns>
    public static ActionResult KickUser(this GroupChannel? channel, IUser? user, IUser removedBy)
    {
        if (channel == null)
            return ActionResult.ChannelNotFound;
        if (user == null)
            return ActionResult.UserNotFound;
        if (!channel.Users.Contains(user))
            return new ActionResult { Error = ErrorType.UserNotInChannel };
        bool removingUserIsInChannel = channel.Users.Contains(removedBy);
        bool removingUserIsAdmin = channel.Admins.Contains(removedBy);
        if (!removingUserIsInChannel || !removingUserIsAdmin)
            return new ActionResult { Error = ErrorType.InsufficientPermissions };
        channel.Users.Remove(user);
        user.Channels.Remove(channel.Id);
        OnMemberChanged(channel, user, ChannelMemberChangeType.Left);
        return ActionResult.SuccessResult;
    }

    /// <summary>
    /// Leaves a group channel as the given user.
    /// </summary>
    /// <param name="user">The user that wants to leave the given group</param>
    /// <param name="channel">The group channel to leave from.</param>
    /// <returns>If the action was successful or not, and the error that occured if it was not successful</returns>
    public static ActionResult LeaveGroup(this IUser? user, GroupChannel? channel)
    {
        if (channel == null) return ActionResult.ChannelNotFound;
        if (user == null) return ActionResult.UserNotFound;
        if (!channel.Users.Contains(user)) return new ActionResult { Error = ErrorType.UserNotInChannel };
        channel.Users.Remove(user);
        user.Channels.Remove(channel.Id);
        OnMemberChanged(channel, user, ChannelMemberChangeType.Left);
        return ActionResult.SuccessResult;
    }

    private static void OnMemberChanged(IChannel channel, IUser user, ChannelMemberChangeType changeType)
    {
        MemberChanged?.Invoke(channel, user, changeType);
    }

    private static void OnMessageSent(IChannel channel, Message message)
    {
        MessageSent?.Invoke(channel, message);
    }

    private static void OnGroupNameChanged(GroupChannel channel, string name)
    {
        GroupNameChanged?.Invoke(channel, name);
    }

    private static void OnGroupDescriptionChanged(GroupChannel channel, string description)
    {
        GroupDescriptionChanged?.Invoke(channel, description);
    }

    private static void OnChannelCreated(IChannel channel)
    {
        ChannelCreated?.Invoke(channel);
    }
}