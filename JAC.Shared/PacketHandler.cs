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
    protected Dictionary<string, Func<PacketBase, Task>> PacketHandlers { get; init; } = new();

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
    
    protected Task OnPacketFragmentReceived(PacketBase packetBase)
    {
        var packet = packetBase as FragmentPacket;
        if (packet == null) return Task.CompletedTask;
        if (!PacketFragmentCache.ContainsKey(packet.Id))
            PacketFragmentCache.Add(packet.Id, new List<FragmentPacket>());
        if (!AllFragmentsReceived(packet.Id)) return Task.CompletedTask;
        var fragments = PacketFragmentCache[packet.Id];
        var fullPacket = fragments.AssemblePacket();
        return HandleAsync(fullPacket);
    }

    public JsonSerializerOptions JsonSerializerOptions { get; init; } = new();

    /// <summary>
    /// Handles an incoming packet by calling the appropriate method based on the packet prefix.
    /// </summary>
    /// <param name="packet">The packet to handle</param>
    public async Task HandleAsync(PacketBase packet)
    {
        var prefix = packet.GetPrefix();
        Console.WriteLine($"------------------------\n\rPacket received: {prefix}\n\r------------------------\n\r");
        if (PacketHandlers.TryGetValue(prefix, out var handler))
            await handler(packet);
    }
}