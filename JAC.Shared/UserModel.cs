namespace JAC.Shared;

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