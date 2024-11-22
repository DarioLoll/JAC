using System;
using System.Linq;
using System.Threading.Tasks;
using JAC.Shared;
using JAC.Shared.Channels;
using JAC.Shared.Packets;
using JAC.ViewModels;
using JAC.Views;

namespace JAC.Models;

/// <summary>
/// <inheritdoc cref="PacketHandler"/> - from a client's perspective
/// </summary>
public class ClientPacketHandler : PacketHandler
{
    /// <summary>
    /// Occurs when this client logs in successfully
    /// </summary>
    public event Func<LoginSuccessPacket, Task>? LoginSucceeded;
    
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

    

    private Task OnServerShutDown(PacketBase packetBase)
    {
        //later, some kind of notification will be displayed
        return Task.CompletedTask;
    }

    private Task OnNewMessagesReceived(PacketBase packetBase)
    {
        var packet = packetBase as GetNewMessagesResponsePacket;
        if (packet == null) return Task.CompletedTask;
        var directory = ChatClient.Instance.Directory;
        foreach (var channelId in packet.Messages.Keys)   
        {
            var channel = directory?.GetChannel(channelId);
            if (channel == null) return Task.CompletedTask;
            var messages = packet.Messages[channelId].ToList();
            messages.Sort();
            foreach (var message in messages)
            {
                channel.AddMessage(message);
            }
        }

        return Task.CompletedTask;
    }

    private Task OnMessageReceived(PacketBase packetBase)
    {
        var packet = packetBase as MessageReceivedPacket;
        if (packet == null) return Task.CompletedTask;
        var directory = ChatClient.Instance.Directory;
        var channel = directory?.GetChannel(packet.ChannelId);
        if (channel == null) return Task.CompletedTask;
        channel.AddMessage(packet.Message);
        return Task.CompletedTask;
    }

    private async Task OnLoginSucceeded(PacketBase packetBase)
    {
        var packet = packetBase as LoginSuccessPacket;
        if (packet == null) return;
        LoginSucceeded?.Invoke(packet);
        await OnChannelsReceived(packet);
    }

    private async Task OnChannelsReceived(PacketBase packetBase)
    {
        var packet = (LoginSuccessPacket?)packetBase;
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
        MainWindow.Instance.SwitchToViewModel(new MainViewModel());
    }

    private Task OnError(PacketBase packetBase)
    {
        ErrorPacket? packet = packetBase as ErrorPacket;
        if (packet == null) return Task.CompletedTask;
        MainWindow.Instance.CurrentViewModel.DisplayError(packet.ErrorType);
        return Task.CompletedTask;
    }

    private Task OnChannelRemoved(PacketBase packetBase)
    {
        var packet = packetBase as ChannelRemovedPacket;
        if (packet == null) return Task.CompletedTask;
        var directory = ChatClient.Instance.Directory;
        directory?.RemoveChannel(packet.RemovedChannelId);
        return Task.CompletedTask;
    }

    private Task OnChannelNameChanged(PacketBase packetBase)
    {
        var packet = packetBase as ChannelNameChangedPacket;
        if (packet == null) return Task.CompletedTask;
        var directory = ChatClient.Instance.Directory;
        var channel = directory?.GetChannel(packet.ChannelId);
        if (channel is not GroupChannel gc) return Task.CompletedTask;
        gc.Name = packet.NewName;
        return Task.CompletedTask;
    }

    private Task OnChannelMembersChanged(PacketBase packetBase)
    {
        var packet = packetBase as ChannelMembersChangedPacket;
        if (packet == null) return Task.CompletedTask;
        var channel = ChatClient.Instance.Directory?.GetChannel(packet.ChannelId);
        var user = packet.User;
        if (channel == null) return Task.CompletedTask;
        switch (packet.ChangeType)
        {
            case ChannelMemberChangeType.Joined:
                channel.AddUser(new User(user)); break;
            case ChannelMemberChangeType.Left:
                channel.RemoveUser(new User(user)); break;
            case ChannelMemberChangeType.RankChanged:
                ((GroupChannel)channel).ChangeUserRank(new User(user)); break;
        }

        return Task.CompletedTask;
    }

    private Task OnChannelDescriptionChanged(PacketBase packetBase)
    {
        var packet = packetBase as ChannelDescriptionChangedPacket;
        if (packet == null) return Task.CompletedTask;
        var directory = ChatClient.Instance.Directory;
        var channel = directory?.GetChannel(packet.ChannelId);
        if (channel is not GroupChannel gc) return Task.CompletedTask;
        gc.Description = packet.NewDescription;
        return Task.CompletedTask;
    }

    private Task OnChannelAdded(PacketBase packetBase)
    {
        var packet = packetBase as ChannelAddedPacket;
        if (packet == null) return Task.CompletedTask;
        var directory = ChatClient.Instance.Directory;
        var channelModel = packet.NewChannel;
        var channel = channelModel switch
        {
            GroupChannelProfile gcm => new GroupChannel(gcm),
            _ => new BaseChannel(channelModel)
        };
        directory?.AddChannel(channel);
        return Task.CompletedTask;
    }
}