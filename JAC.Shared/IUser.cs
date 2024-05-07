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
    /// The date and time when the user was last seen online.
    /// </summary>
    DateTime LastSeen { get; set; }
    
    /// <summary>
    /// If the user is currently online.
    /// </summary>
    bool IsOnline { get; set; }
}