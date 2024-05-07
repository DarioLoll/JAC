namespace JAC.Shared.Packets;

/// <summary>
/// A packet that contains a fragment of a larger packet.
/// </summary>
public class FragmentPacket : PacketBase, IComparable<FragmentPacket>
{
    /// <summary>
    /// The data of the fragment.
    /// </summary>
    public required string Data { get; init; }
    
    /// <summary>
    /// The sequence number of the fragment.
    /// </summary>
    public required int SequenceNumber { get; init; }

    /// <summary>
    /// If this is the last fragment.
    /// </summary>
    public bool IsLast { get; init; }

    /// <summary>
    /// The id of the packet that this fragment is a part of.
    /// </summary>
    public required ushort Id { get; init; }

    /// <summary>
    /// Compares this fragment to another fragment by their sequence numbers.
    /// </summary>
    /// <param name="other">The fragment to compare to.</param>
    /// <returns>The result of the comparison.</returns>
    public int CompareTo(FragmentPacket? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (ReferenceEquals(null, other)) return 1;
        return SequenceNumber.CompareTo(other.SequenceNumber);
    }
}