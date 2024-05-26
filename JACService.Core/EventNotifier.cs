using JAC.Shared;
using JAC.Shared.Packets;
using JACService.Core.Contracts;

namespace JACService.Core;

/// <summary>
/// Responsible for notifying clients of relevant events 
/// <example>When a user joins a channel,
/// the other users in that channel should be notified of it so they can update their UIs</example>
/// </summary>
public class EventNotifier
{
    public static EventNotifier Instance { get; } = new();
    
    private static IServiceLogger? _logger;

    private EventNotifier() { }
    
    public void Initialize()
    {
        _logger = Server.Instance.Logger;
        ChatServiceDirectory.Instance.UserJoinedChannel += OnUserJoinedChannel;
        ChatServiceDirectory.Instance.UserLeftChannel += OnUserLeftChannel;
        ChatServiceDirectory.Instance.UserRankChanged += OnUserRankChanged;
        ChatServiceDirectory.Instance.MessageSent += OnMessageSent;
        ChatServiceDirectory.Instance.GroupNameChanged += OnGroupNameChanged;
        ChatServiceDirectory.Instance.GroupDescriptionChanged += OnGroupDescriptionChanged;
    }


    private void OnGroupDescriptionChanged(GroupChannel group)
    {
        _logger?.LogServiceInfoAsync($"Group {group.Id} description changed to \"{group.Description}\".");
        var updatePacket = new ChannelDescriptionChangedPacket
        {
            ChannelId = group.Id,
            NewDescription = group.Description
        };
        Server.Instance.ClientManager?.BroadCastAsync(group.OnlineUsers, updatePacket);
    }

    private void OnGroupNameChanged(GroupChannel group)
    {
        _logger?.LogServiceInfoAsync($"Group {group.Id} name changed to \"{group.Name}\".");
        var updatePacket = new ChannelNameChangedPacket
        {
            ChannelId = group.Id,
            NewName = group.Name
        };
        Server.Instance.ClientManager?.BroadCastAsync(group.OnlineUsers, updatePacket);
    }

    private void OnMessageSent(BaseChannel channel, Message message)
    {
        var updatePacket = new MessageReceivedPacket
        {
            ChannelId = channel.Id,
            Message = message
        };
        Server.Instance.ClientManager?.BroadCastAsync(channel.OnlineUsers, updatePacket);
    }

    private void OnUserRankChanged(ChatUser user, BaseChannel channel)
    {
        _logger?.LogServiceInfoAsync($"User {user.Nickname} rank in channel {channel.Id} changed.");
        var updatePacket = new ChannelMembersChangedPacket
        {
            ChangeType = ChannelMemberChangeType.RankChanged,
            ChannelId = channel.Id,
            User = user.ToUserModel()
        };
        Server.Instance.ClientManager?.BroadCastAsync(channel.OnlineUsers, updatePacket);
    }

    private void OnUserLeftChannel(ChatUser user, BaseChannel channel)
    {
        _logger?.LogServiceInfoAsync($"User {user.Nickname} left channel {channel.Id}.");
        var updatePacket = new ChannelMembersChangedPacket
        {
            ChangeType = ChannelMemberChangeType.Left,
            ChannelId = channel.Id,
            User = user.ToUserModel()
        };
        Server.Instance.ClientManager?.BroadCastAsync(channel.OnlineUsers, updatePacket);
        
        var userUpdatePacket = new ChannelRemovedPacket
        {
            RemovedChannelId = channel.Id
        };
        Server.Instance.ClientManager?.SendToUserAsync(user, userUpdatePacket);
    }

    private void OnUserJoinedChannel(ChatUser user, BaseChannel channel)
    {
        _logger?.LogServiceInfoAsync($"User {user.Nickname} joined channel {channel.Id}.");
        var groupUpdatePacket = new ChannelMembersChangedPacket
        {
            ChangeType = ChannelMemberChangeType.Joined,
            ChannelId = channel.Id,
            User = user.ToUserModel()
        };
        var usersToUpdate = channel.OnlineUsers.Where(u => u != user);
        Server.Instance.ClientManager?.BroadCastAsync(usersToUpdate, groupUpdatePacket);

        var userUpdatePacket = new ChannelAddedPacket
        {
            NewChannel = channel.ToCorrespondingChannelModel()
        };
        Server.Instance.ClientManager?.SendToUserAsync(user, userUpdatePacket);
    }
}