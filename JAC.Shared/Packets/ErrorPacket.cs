namespace JAC.Shared.Packets;

/// <summary>
/// Packet to notify clients of an error.
/// </summary>
public class ErrorPacket : PacketBase
{
    /// <summary>
    /// The type of error that occurred.
    /// </summary>
    public ErrorType ErrorType { get; init; }
}
