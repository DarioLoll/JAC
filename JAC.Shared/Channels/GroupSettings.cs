namespace JAC.Shared.Channels;

/// <summary>
/// Represents the settings for a <see cref="GroupChannel"/>
/// </summary>
public struct GroupSettings
{
    public GroupSettings()
    {
    }

    /// <summary>
    /// If only admins can send messages to the group.
    /// <remarks>Default is false</remarks>
    /// </summary>
    public bool ReadOnlyForMembers { get; set; } = false;
    
    /// <summary>
    /// If everyone, including non-admins, can add new members to the group.
    /// <remarks>Default is true</remarks>
    /// </summary>
    public bool AllowMembersToAdd { get; set; } = true;
    
    /// <summary>
    /// If everyone, including non-admins, can change the name of the group.
    /// <remarks>Default is false</remarks>
    /// </summary>
    public bool AllowMembersToChangeName { get; set; } = false;
    
    /// <summary>
    /// If everyone, including non-admins, can change the description of the group.
    /// <remarks>Default is true</remarks>
    /// </summary>
    public bool AllowMembersToChangeDescription { get; set; } = true;
}