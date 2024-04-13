namespace JAC.Shared.Packets;

/// <summary>
/// Packet to notify a client that a message has been received.
/// </summary>
public class MessageReceivedPacket : PacketBase
{
    /// <summary>
    /// The id of the channel that the message was sent to.
    /// </summary>
    public required ulong ChannelId { get; init; }
    
    /// <summary>
    /// The message that was received.
    /// </summary>
    public required Message Message { get; init; }
}