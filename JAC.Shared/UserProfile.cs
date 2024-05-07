namespace JAC.Shared;

/// <summary>
/// Represents a model of a user that is used to send users between the server and the client
/// </summary>
public class UserProfile
{
    /// <summary>
    /// <inheritdoc cref="IUser.Nickname"/>
    /// </summary>
    public required string Nickname { get; init; }

    /// <summary>
    /// <inheritdoc cref="IUser.LastSeen"/>
    /// </summary>
    public DateTime LastSeen { get; set; }

    /// <summary>
    /// <inheritdoc cref="IUser.IsOnline"/>
    /// </summary>
    public bool IsOnline { get; set; }

    //later on more properties like profile picture, status, etc. can be added
}