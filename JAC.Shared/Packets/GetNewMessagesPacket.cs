namespace JAC.Shared.Packets;

/// <summary>
/// Packet to request new messages in a channel from the server since the last login.
/// </summary>
public class GetNewMessagesPacket : PacketBase
{
    /// <summary>
    /// The ids of the channels to get new messages from.
    /// </summary>
    public required IList<ulong> ChannelIds { get; init; }
}