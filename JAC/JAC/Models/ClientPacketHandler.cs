using System;
using JAC.Shared;
using JAC.Shared.Packets;

namespace JAC.Models;

public class ClientPacketHandler : PacketHandler
{
    public event Action<ChannelAddedPacket>? ChannelAdded;
    
    public event Action<ChannelDescriptionChangedPacket>? ChannelDescriptionChanged;
    
    public event Action<ChannelMembersChangedPacket>? ChannelMembersChanged;
    
    public event Action<ChannelNameChangedPacket>? ChannelNameChanged;
    
    public event Action<ChannelRemovedPacket>? ChannelRemoved;
    
    public event Action<GetChannelsResponsePacket>? ChannelsReceived;
    
    public event Action<LoginSuccessPacket>? LoginSucceeded;
    
    public event Action<MessageReceivedPacket>? MessageReceived;
    
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
            { PacketBase.GetPrefix<LoginSuccessPacket>(), OnLoginSucceeded },
            { PacketBase.GetPrefix<MessageReceivedPacket>(), OnMessageReceived },
        };
    }

    private void OnMessageReceived(string packetJson)
    {
        var packet = PacketBase.FromJson<MessageReceivedPacket>(packetJson, JsonSerializerOptions);
        if (packet == null) return;
        MessageReceived?.Invoke(packet);
    }

    private void OnLoginSucceeded(string packetJson)
    {
        var packet = PacketBase.FromJson<LoginSuccessPacket>(packetJson, JsonSerializerOptions);
        if (packet == null) return;
        LoginSucceeded?.Invoke(packet);
    }

    private void OnChannelsReceived(string packetJson)
    {
        var packet = PacketBase.FromJson<GetChannelsResponsePacket>(packetJson, JsonSerializerOptions);
        if (packet == null) return;
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
        ChannelRemoved?.Invoke(packet);
    }

    private void OnChannelNameChanged(string packetJson)
    {
        var packet = PacketBase.FromJson<ChannelNameChangedPacket>(packetJson, JsonSerializerOptions);
        if (packet == null) return;
        ChannelNameChanged?.Invoke(packet);
    }

    private void OnChannelMembersChanged(string packetJson)
    {
        var packet = PacketBase.FromJson<ChannelMembersChangedPacket>(packetJson, JsonSerializerOptions);
        if (packet == null) return;
        ChannelMembersChanged?.Invoke(packet);
    }

    private void OnChannelDescriptionChanged(string packetJson)
    {
        var packet = PacketBase.FromJson<ChannelDescriptionChangedPacket>(packetJson, JsonSerializerOptions);
        if (packet == null) return;
        ChannelDescriptionChanged?.Invoke(packet);
    }

    private void OnChannelAdded(string packetJson)
    {
        var packet = PacketBase.FromJson<ChannelAddedPacket>(packetJson, JsonSerializerOptions);
        if (packet == null) return;
        ChannelAdded?.Invoke(packet);
    }
}