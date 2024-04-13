namespace JAC.Shared.Packets;

/// <summary>
/// Packet to send a message to a channel.
/// </summary>
public class SendMessagePacket : PacketBase
{
    /// <summary>
    /// The id of the channel to send the message to.
    /// </summary>
    public required ulong ChannelId { get; init; }
    
    /// <summary>
    /// The message to send to the channel.
    /// </summary>
    public required string Message { get; init; }
}