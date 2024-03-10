namespace JAC.Shared;

public class PacketHandler
{
    public Dictionary<string, Action<string>> PacketHandlers { get; } = new();

    public void Handle(string line)
    {
        var parts = line.Split(' ', 2);
        if (parts.Length != 2) return;
        var prefix = parts[0];
        var json = parts[1];
        if (PacketHandlers.TryGetValue(prefix, out var handler))
            handler(json);
    }
}