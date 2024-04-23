namespace JAC.Shared;

/// <summary>
/// Represents a model of a user that is used to send users between the server and the client
/// </summary>
public class UserModel : IUser
{
    /// <summary>
    /// <inheritdoc cref="IUser.Nickname"/>
    /// </summary>
    public required string Nickname { get; init; }
    
    /// <summary>
    /// <inheritdoc cref="IUser.Channels"/>
    /// </summary>
    public IList<ulong> Channels { get; init; } = new List<ulong>();
}