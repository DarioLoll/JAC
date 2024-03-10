using System.Runtime.Serialization;
using System.Text.Json;

namespace JAC.Shared;

public abstract class PacketBase
{
    public static string GetPrefix(Type packetType)
    {
        if (!packetType.IsSubclassOf(typeof(PacketBase)))
            throw new ArgumentException("T must be a subclass of PacketBase");
        var typeName = packetType.Name;
        var typeNameWithoutPacket = typeName.Substring(0, typeName.Length - "Packet".Length);
        var prefix = "/" + typeNameWithoutPacket.ToLower();
        return prefix;
    }
    
    public static string GetPrefix<T>() where T : PacketBase => GetPrefix(typeof(T));
    
    public abstract string ToJson();
    
    public static TPacket? FromJson<TPacket>(string json) where TPacket : PacketBase => JsonSerializer.Deserialize<TPacket>(json);
    
    public static PacketBase? FromJson(string json) => JsonSerializer.Deserialize<PacketBase>(json);
    
    public static string CutPrefix(string request) => request.Split(' ', 2)[1];
}