namespace JAC.Shared.Packets;

public class CreateGroupPacket : PacketBase
{
    public required string Name { get; init; }
    public required string Description { get; init; }

}