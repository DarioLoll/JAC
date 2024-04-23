using System.Runtime.Serialization;
using System.Text.Json;

namespace JAC.Shared;

/// <summary>
/// Represents the base class for all packets,
/// which are used to standardize the communication between the client and the server.
/// A packet is just a class that is serialized to JSON and sent to the other side.
/// It contains a prefix that identifies it, and parameters that are specific to the packet.
/// </summary>
public class PacketBase
{
    /// <summary>
    /// Gets the prefix of the packet type by removing the "Packet" suffix,
    /// converting it to lowercase and prefixing it with a slash.
    /// </summary>
    /// <param name="packetType">The type of the packet (must inherit from PacketBase)</param>
    /// <returns>A string in this form: "/{prefix}"</returns>
    /// <exception cref="ArgumentException">In case the given packet type does not inherit from PacketBase</exception>
    public static string GetPrefix(Type packetType)
    {
        if (!packetType.IsSubclassOf(typeof(PacketBase)) && packetType != typeof(PacketBase))
            throw new ArgumentException("T must be a subclass of PacketBase");
        var typeName = packetType.Name;
        var typeNameWithoutPacket = typeName.Substring(0, typeName.Length - "Packet".Length);
        var prefix = "/" + typeNameWithoutPacket.ToLower();
        return prefix;
    }
    
    /// <summary>
    /// If a packet does not require any parameters, this property will contain the type of the packet.
    /// Otherwise, it will be null.
    /// </summary>
    public ParameterlessPacket? ParameterlessPacketType { get; }
    
    /// <summary>
    /// Gets the prefix of a parameterless packet.
    /// </summary>
    /// <param name="parameterlessPacketType">the type of the packet</param>
    /// <returns>A string in this form: "/{prefix}"</returns>
    public static string GetPrefix(ParameterlessPacket parameterlessPacketType)
    {
        return "/" + parameterlessPacketType.ToString().ToLower();
    }
    
    public PacketBase()
    {
        ParameterlessPacketType = null;
    }

    public PacketBase(ParameterlessPacket parameterlessPacketType)
    {
        ParameterlessPacketType = parameterlessPacketType;
    }
    
    /// <summary>
    /// Gets the prefix of the packet type by removing the "Packet" suffix,
    /// converting it to lowercase and prefixing it with a slash.
    /// </summary>
    /// <typeparam name="T">The type of the packet (must inherit from PacketBase)</typeparam>
    /// <returns>A string in this form: "/{prefix}"</returns>
    /// <exception cref="ArgumentException">In case the given packet type does not inherit from PacketBase</exception>
    public static string GetPrefix<T>() where T : PacketBase => GetPrefix(typeof(T));
    
    /// <summary>
    /// Converts the json string to a packet of the specified type.
    /// </summary>
    /// <typeparam name="TPacket">The type of the packet the json represents</typeparam>
    /// <returns>A packet created from the json</returns>
    public static TPacket? FromJson<TPacket>(string json, JsonSerializerOptions? options = null) where TPacket : PacketBase
    {
        return JsonSerializer.Deserialize<TPacket>(json, options);
    }

    public static PacketBase? FromJson(string json, JsonSerializerOptions? options = null)
    {
        return JsonSerializer.Deserialize<PacketBase>(json, options);
    }

    private static string CutPrefix(string request) => request.Split(' ', 2)[1];
}

/// <summary>
/// List of packets that do not require any parameters.
/// </summary>
public enum ParameterlessPacket
{
    GetChannels,
}
