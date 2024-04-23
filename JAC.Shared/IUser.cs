namespace JAC.Shared;

public interface IUser
{
    /// <summary>
    /// The unique identifier of the user.
    /// </summary>
    string Nickname { get; init; }
    
    /// <summary>
    /// List of channel ids that the user is a member of.
    /// </summary>
    IList<ulong> Channels { get; init; }
}