namespace JAC.Shared.Channels;

/// <summary>
/// Represents a model of a channel that is used to send channels between the server and the client
/// </summary>
public class ChannelModelBase
{
    /// <summary>
    /// <inheritdoc cref="IChannel.Id"/>
    /// </summary>
    public required ulong Id { get; init; }
    
    /// <summary>
    /// <inheritdoc cref="IChannel.Users"/>
    /// </summary>
    public IList<UserModel> Users { get; init; } = new List<UserModel>();
    
    /// <summary>
    /// <inheritdoc cref="IChannel.Messages"/>
    /// </summary>
    public IList<Message> Messages { get; init; } = new List<Message>();
    
    /// <summary>
    /// <inheritdoc cref="IChannel.Created"/>
    /// </summary>
    public required DateTime Created { get; init; }
}