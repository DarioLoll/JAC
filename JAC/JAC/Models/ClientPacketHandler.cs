using System;
using JAC.Shared;
using JAC.Shared.Packets;

namespace JAC.Models;

/// <summary>
/// <inheritdoc cref="PacketHandler"/> - from a client's perspective
/// </summary>
public class ClientPacketHandler : PacketHandler
{
    /// <summary>
    /// Occurs when the user this client is logged in as is added to a channel
    /// </summary>
    public event Action<ChannelAddedPacket>? ChannelAdded;
    /// <summary>
    /// Occurs when the description of a channel this client is in is changed
    /// </summary>
    public event Action<ChannelDescriptionChangedPacket>? ChannelDescriptionChanged;
    /// <summary>
    /// Occurs when a member of a channel this client is in have changed
    /// </summary>
    public event Action<ChannelMembersChangedPacket>? ChannelMembersChanged;
    /// <summary>
    /// Occurs when the name of a channel this client is in is changed
    /// </summary>
    public event Action<ChannelNameChangedPacket>? ChannelNameChanged;
    /// <summary>
    /// Occurs when the user this client is logged in as is removed from a channel
    /// </summary>
    public event Action<ChannelRemovedPacket>? ChannelRemoved;
    /// <summary>
    /// Occurs when the server responds to the client's request for channels
    /// </summary>
    public event Action<GetChannelsResponsePacket>? ChannelsReceived;
    /// <summary>
    /// Occurs when this client logs in successfully
    /// </summary>
    public event Action<LoginSuccessPacket>? LoginSucceeded;
    /// <summary>
    /// Occurs when there has been a message sent to a channel this client is in
    /// </summary>
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