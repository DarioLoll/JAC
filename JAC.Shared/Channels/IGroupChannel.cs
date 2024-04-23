namespace JAC.Shared.Channels;

/// <summary>
/// Represents a group chat with multiple users, admins, and settings.
/// </summary>
public interface IGroupChannel : IChannel
{
    /// <summary>
    /// The name of the group. Not necessarily unique. Able to be changed by members
    /// <remarks>Settings can be adjusted so that only admins can edit the description</remarks>
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Stores more info about the group that members can edit.
    /// <remarks>Settings can be adjusted so that only admins can edit the description</remarks>
    /// </summary>
    public string Description { get; set; }
    
    /// <summary>
    /// List of users who possess admin privileges in the group.
    /// </summary>
    public IList<IUser> Admins { get; init; }
    
    /// <summary>
    /// <inheritdoc cref="GroupSettings"/>
    /// </summary>
    public GroupSettings Settings { get; init; }

    
}