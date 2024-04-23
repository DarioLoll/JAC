namespace JAC.Shared.Channels;

/// <summary>
/// Represents a model of a group channel that is used to send group channels between the server and the client
/// </summary>
public class GroupChannelModel : ChannelModelBase, IGroupChannel
{
    /// <summary>
    /// <inheritdoc cref="IGroupChannel.Name"/>
    /// </summary>
    public required string Name { get; set; }
    
    /// <summary>
    /// <inheritdoc cref="IGroupChannel.Description"/>
    /// </summary>
    public string Description { get; set; } = "";
    
    /// <summary>
    /// <inheritdoc cref="IGroupChannel.Admins"/>
    /// </summary>
    public IList<IUser> Admins { get; init; } = new List<IUser>();
    
    /// <summary>
    /// <inheritdoc cref="IGroupChannel.Settings"/>
    /// </summary>
    public GroupSettings Settings { get; init; } = new GroupSettings();
}