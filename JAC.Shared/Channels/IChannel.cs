namespace JAC.Shared.Channels;

/// <summary>
/// Represents a chat channel, which is a group of users that can send messages to each other.
/// </summary>
public interface IChannel
{
    //There is no name property in the interface, because not all channels have names.
    //A private channel (a channel between only two users), for example, does not have a name.
    //The name of a private channel is determined individually by both users in it, simply showing the name of the other user.
    
    /// <summary>
    /// Each channel has a unique identifier, since it is possible to have multiple channels with the same name.
    /// </summary>
    ulong Id { get; init; }
    
    /// <summary>
    /// A list of all users in the channel.
    /// </summary>
    IList<IUser> Users { get; init; }
    
    /// <summary>
    /// A list of all messages sent in the channel.
    /// </summary>
    IList<Message> Messages { get; init; }
    
    /// <summary>
    /// When the channel was created.
    /// </summary>
    DateTime Created { get; init; }

}