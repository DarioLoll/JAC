using System.Text.Json;

namespace JAC.Shared;

/// <summary>
/// Handles incoming packets by mapping packet prefixes to a method that handles that type of packet.
/// </summary>
public class PacketHandler
{
    /// <summary>
    /// The mappings of packet prefixes to methods that handle those.
    /// </summary>
    public Dictionary<string, Action<string>> PacketHandlers { get; protected init; } = new();

    public JsonSerializerOptions JsonSerializerOptions { get; init; } = new();

    /// <summary>
    /// Handles an incoming packet by calling the appropriate method based on the packet prefix.
    /// </summary>
    /// <param name="packetAsString">The string received on the socket which looks like this: "/{prefix} {json}"</param>
    public void Handle(string packetAsString)
    {
        var parts = packetAsString.Split(' ', 2);
        if (parts.Length < 1) return;
        var prefix = parts[0];
        var parameters = parts.Length > 1 ? parts[1] : string.Empty;
        if (PacketHandlers.TryGetValue(prefix, out var handler))
            handler(parameters);
    }
}