namespace JAC.Shared.Packets;

/// <summary>
/// Packet to respond to a client's request for new messages in a channel. See <see cref="GetNewMessagesPacket"/>
/// </summary>
public class GetNewMessagesResponsePacket : PacketBase
{
    /// <summary>
    /// The messages that have been sent to each channel since the last login.
    /// </summary>
    public required IDictionary<ulong, IEnumerable<Message>> Messages { get; init; }
}