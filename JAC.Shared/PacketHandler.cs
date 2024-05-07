using System.Text.Json;
using JAC.Shared.Packets;

namespace JAC.Shared;

/// <summary>
/// Handles incoming packets by mapping packet prefixes to a method that handles that type of packet.
/// </summary>
public class PacketHandler
{
    /// <summary>
    /// The mappings of packet prefixes to methods that handle those.
    /// </summary>
    protected Dictionary<string, Action<string>> PacketHandlers { get; init; } = new();

    /// <summary>
    /// A cache of packet fragments that are waiting for all fragments to arrive.
    /// <remarks>The key is the id of a packet, and the values assigned to it are the fragments that arrived</remarks>
    /// </summary>
    protected Dictionary<ushort, List<FragmentPacket>> PacketFragmentCache { get; } = new();
    
    protected bool AllFragmentsReceived(ushort id)
    {
        var fragments = PacketFragmentCache[id];
        var lastFragment = fragments.FirstOrDefault(fragment => fragment.IsLast);
        var lastFragmentArrived = lastFragment != null;
        if (!lastFragmentArrived) return false;
        var lastSequenceNumber = lastFragment!.SequenceNumber;
        var allSequenceNumbersArrived = true;
        for (var i = 0; i <= lastSequenceNumber; i++)
        {
            if (fragments.All(fragment => fragment.SequenceNumber != i))
            {
                allSequenceNumbersArrived = false;
                break;
            }
        }
        return allSequenceNumbersArrived;
    }
    
    protected void OnPacketFragmentReceived(string json)
    {
        var packet = PacketBase.FromJson<FragmentPacket>(json, JsonSerializerOptions);
        if (packet == null) return;
        if (!PacketFragmentCache.ContainsKey(packet.Id))
            PacketFragmentCache.Add(packet.Id, new List<FragmentPacket>());
        if (!AllFragmentsReceived(packet.Id)) return;
        var fragments = PacketFragmentCache[packet.Id];
        var fullPacket = fragments.AssemblePacket();
        Handle(fullPacket);
    }

    public JsonSerializerOptions JsonSerializerOptions { get; init; } = new();

    /// <summary>
    /// Handles an incoming packet by calling the appropriate method based on the packet prefix.
    /// </summary>
    /// <param name="packetAsString">The string received on the socket which looks like this: "/{prefix} {json}"</param>
    public void Handle(string packetAsString)
    {
        Console.WriteLine($"------------------------\n\rPacket received: {packetAsString}\n\r------------------------\n\r");
        var parts = packetAsString.Split(' ', 2);
        if (parts.Length < 1) return;
        var prefix = parts[0];
        var parameters = parts.Length > 1 ? parts[1] : string.Empty;
        if (PacketHandlers.TryGetValue(prefix, out var handler))
            handler(parameters);
    }
}