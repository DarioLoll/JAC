namespace JAC.Shared.Packets;

public class ChannelMembersChangedPacket : PacketBase
{
    public required ulong ChannelId { get; init; }
    public required IUser User { get; init; }
    public required ChannelMemberChangeType ChangeType { get; init; } 
}

public enum ChannelMemberChangeType
{
    Joined,
    Left,
    RankChanged
}