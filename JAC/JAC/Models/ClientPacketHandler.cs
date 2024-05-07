using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using JAC.Shared;
using JAC.Shared.Channels;
using JAC.Shared.Packets;

namespace JAC.Models;

/// <summary>
/// <inheritdoc cref="PacketHandler"/> - from a client's perspective
/// </summary>
public class ClientPacketHandler : PacketHandler
{
    /// <summary>
    /// Occurs when the server responds to the client's request for channels
    /// </summary>
    public event Action<GetChannelsResponsePacket>? ChannelsReceived;
    /// <summary>
    /// Occurs when this client logs in successfully
    /// </summary>
    public event Action<LoginSuccessPacket>? LoginSucceeded;
    
    public ClientPacketHandler()
    {
        PacketHandlers = new()
        {
            { PacketBase.GetPrefix<ChannelAddedPacket>(), OnChannelAdded },
            { PacketBase.GetPrefix<ChannelDescriptionChangedPacket>(), OnChannelDescriptionChanged },
            { PacketBase.GetPrefix<ChannelMembersChangedPacket>(), OnChannelMembersChanged },
            { PacketBase.GetPrefix<ChannelNameChangedPacket>(), OnChannelNameChanged },
            { PacketBase.GetPrefix<ChannelRemovedPacket>(), OnChannelRemoved },
            { PacketBase.GetPrefix<ErrorPacket>(), OnError },
            { PacketBase.GetPrefix<GetChannelsResponsePacket>(), OnChannelsReceived },
            { PacketBase.GetPrefix<GetNewMessagesResponsePacket>(), OnNewMessagesReceived },
            { PacketBase.GetPrefix<LoginSuccessPacket>(), OnLoginSucceeded },
            { PacketBase.GetPrefix<MessageReceivedPacket>(), OnMessageReceived },
            { PacketBase.GetPrefix(ParameterlessPacket.Disconnect), OnServerShutDown },
            { PacketBase.GetPrefix<FragmentPacket>(), OnPacketFragmentReceived }
        };
    }

    

    private void OnServerShutDown(string packetJson)
    {
        //later, some kind of notification will be displayed
    }

    private void OnNewMessagesReceived(string packetJson)
    {
        var packet = PacketBase.FromJson<GetNewMessagesResponsePacket>(packetJson, JsonSerializerOptions);
        if (packet == null) return;
        var directory = ChatClient.Instance.Directory;
        foreach (var channelId in packet.Messages.Keys)   
        {
            var channel = directory?.GetChannel(channelId);
            if (channel == null) return;
            var messages = packet.Messages[channelId].ToList();
            messages.Sort();
            foreach (var message in messages)
            {
                channel.AddMessage(message);
            }
        }
        
    }

    private void OnMessageReceived(string packetJson)
    {
        var packet = PacketBase.FromJson<MessageReceivedPacket>(packetJson, JsonSerializerOptions);
        if (packet == null) return;
        var directory = ChatClient.Instance.Directory;
        var channel = directory?.GetChannel(packet.ChannelId);
        if (channel == null) return;
        channel.AddMessage(packet.Message);
    }

    private void OnLoginSucceeded(string packetJson)
    {
        var packet = PacketBase.FromJson<LoginSuccessPacket>(packetJson, JsonSerializerOptions);
        if (packet == null) return;
        LoginSucceeded?.Invoke(packet);
    }

    private async void OnChannelsReceived(string packetJson)
    {
        var packet = PacketBase.FromJson<GetChannelsResponsePacket>(packetJson, JsonSerializerOptions);
        if (packet == null) return;
        var directory = ChatClient.Instance.Directory;
        foreach (var savedChannel in directory!.Channels)
        {
            bool thisUserIsStillInSavedChannel = packet.Channels.Any(channel => channel.Id == savedChannel.Id);
            if (!thisUserIsStillInSavedChannel)
                directory.RemoveChannel(savedChannel.Id);
        }
        foreach (var channelModel in packet.Channels)
        {
            bool alreadyAdded = directory.Channels.Any(channel => channel.Id == channelModel.Id);
            if (alreadyAdded)
            {
                var channel = directory.GetChannel(channelModel.Id);
                channel!.UpdateFromModel(channelModel);
            }
            if (channelModel is GroupChannelProfile groupChannel)
            {
                directory.AddChannel(new GroupChannel(groupChannel));
            }
            else
            {
                directory.AddChannel(new BaseChannel(channelModel));
            }
        }
        await ChatClient.Instance.Send(new GetNewMessagesPacket
        {
            ChannelIds = directory.Channels.Select(channel => channel.Id).ToList()
        });
        ChannelsReceived?.Invoke(packet);
    }

    private void OnError(string packetJson)
    {
        ErrorPacket? packet = PacketBase.FromJson<ErrorPacket>(packetJson, JsonSerializerOptions);
        if (packet == null) return;
        Navigator.Instance.CurrentViewModel.DisplayError(packet.ErrorType);
    }

    private void OnChannelRemoved(string packetJson)
    {
        var packet = PacketBase.FromJson<ChannelRemovedPacket>(packetJson, JsonSerializerOptions);
        if (packet == null) return;
        var directory = ChatClient.Instance.Directory;
        directory?.RemoveChannel(packet.RemovedChannelId);
    }

    private void OnChannelNameChanged(string packetJson)
    {
        var packet = PacketBase.FromJson<ChannelNameChangedPacket>(packetJson, JsonSerializerOptions);
        if (packet == null) return;
        var directory = ChatClient.Instance.Directory;
        var channel = directory?.GetChannel(packet.ChannelId);
        if (channel is not GroupChannel gc) return;
        gc.Name = packet.NewName;
    }

    private void OnChannelMembersChanged(string packetJson)
    {
        var packet = PacketBase.FromJson<ChannelMembersChangedPacket>(packetJson, JsonSerializerOptions);
        if (packet == null) return;
        var channel = ChatClient.Instance.Directory?.GetChannel(packet.ChannelId);
        var user = packet.User;
        if (channel == null) return;
        switch (packet.ChangeType)
        {
            case ChannelMemberChangeType.Joined:
                channel.AddUser(new User(user)); break;
            case ChannelMemberChangeType.Left:
                channel.RemoveUser(new User(user)); break;
            case ChannelMemberChangeType.RankChanged:
                ((GroupChannel)channel).ChangeUserRank(new User(user)); break;
        }
    }

    private void OnChannelDescriptionChanged(string packetJson)
    {
        var packet = PacketBase.FromJson<ChannelDescriptionChangedPacket>(packetJson, JsonSerializerOptions);
        if (packet == null) return;
        var directory = ChatClient.Instance.Directory;
        var channel = directory?.GetChannel(packet.ChannelId);
        if (channel is not GroupChannel gc) return;
        gc.Description = packet.NewDescription;
    }

    private void OnChannelAdded(string packetJson)
    {
        var packet = PacketBase.FromJson<ChannelAddedPacket>(packetJson, JsonSerializerOptions);
        if (packet == null) return;
        var directory = ChatClient.Instance.Directory;
        var channelModel = packet.NewChannel;
        var channel = channelModel switch
        {
            GroupChannelProfile gcm => new GroupChannel(gcm),
            _ => new BaseChannel(channelModel)
        };
        directory?.AddChannel(channel);
    }
}