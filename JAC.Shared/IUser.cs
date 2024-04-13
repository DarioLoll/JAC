namespace JAC.Shared;

/// <summary>
/// Represents a user of the chat system (a logged in client).
/// </summary>
public interface IUser
{
    /// <summary>
    /// The unique identifier of the user.
    /// </summary>
    string Nickname { get; }
    
    /// <summary>
    /// List of channel ids that the user is a member of.
    /// </summary>
    List<ulong> Channels { get; }
    
    /// <summary>
    /// If the user is currently online.
    /// </summary>
    bool IsOnline { get; set; }
    
}