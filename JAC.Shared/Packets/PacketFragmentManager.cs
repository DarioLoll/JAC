using System.Text.Json;

namespace JAC.Shared.Packets;

public static class PacketFragmentManager
{
    /// <summary>
    /// Each packet has an ID in case multiple packets are fragmented at the same time,
    /// the receiving end knows which fragments belong to which packet. This ID is not
    /// unique across the entire session, since there are only 65536 possible values.
    /// But it only needs to be unique within a given time frame.
    /// </summary>
    private static ushort _currentPacketId = 0;

    /// <summary>
    /// Fragments a packet into smaller packets <see cref="FragmentPacket"/>.
    /// </summary>
    /// <param name="packetJson">The packet to fragment (including the prefix).</param>
    /// <param name="packetType">The type of the packet to fragment.</param>
    /// <param name="fragmentSize">The size of each fragment in characters.</param>
    /// <returns>The fragments of the packet.</returns>
    public static IEnumerable<FragmentPacket> FragmentIntoPackets(this string packetJson, int fragmentSize)
    {
        var fragments = new List<FragmentPacket>();
        var packetDataLength = packetJson.Length;
        ushort sequenceNumber = 0;
        var isLast = false;
        for (int i = 0; i < packetJson.Length; i += fragmentSize, sequenceNumber++)
        {
            var remaining = packetDataLength - i;
            var nextFragmentSize = remaining < fragmentSize ? remaining : fragmentSize;
            var fragmentData = packetJson.Substring(i, nextFragmentSize);
            if (i + fragmentSize >= packetDataLength)
            {
                isLast = true;
            }
            fragments.Add(new FragmentPacket
            {
                Id = _currentPacketId,
                SequenceNumber = sequenceNumber,
                IsLast = isLast,
                Data = fragmentData,
            });
        }
        _currentPacketId++;
        return fragments;
    }
    
    /// <summary>
    /// Assembles a packet from fragments.
    /// </summary>
    /// <param name="fragments">The fragments making up the packet.</param>
    /// <returns>The assembled packet as "/{prefix} {packet json}".</returns>
    public static string AssemblePacket(this IEnumerable<FragmentPacket> fragments)
    {
        var fragmentsList = fragments.ToList();
        fragmentsList.Sort();
        var packetData = string.Join("", fragmentsList.Select(f => f.Data));
        return packetData;
    }
}