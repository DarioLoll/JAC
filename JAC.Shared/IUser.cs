namespace JAC.Shared;

public interface IUser
{
    string Nickname { get; }
    
    List<ulong> Channels { get; }

    bool IsOnline { get; }
    
}