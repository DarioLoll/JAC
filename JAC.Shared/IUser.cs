namespace JAC.Shared;

/// <summary>
/// Represents a user of the chat service.
/// </summary>
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