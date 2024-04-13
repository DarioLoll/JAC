namespace JAC.Shared.Channels;

/// <summary>
/// Represents the settings for a <see cref="GroupChannel"/>
/// </summary>
public class GroupSettings
{
    /// <summary>
    /// If only admins can send messages to the group.
    /// </summary>
    public bool ReadOnlyForMembers { get; set; }
    
    /// <summary>
    /// If everyone, including non-admins, can add new members to the group.
    /// <remarks>Default is true</remarks>
    /// </summary>
    public bool AllowMembersToAdd { get; set; } = true;
    
    /// <summary>
    /// If everyone, including non-admins, can change the name and description of the group.
    /// <remarks>Default is true</remarks>
    /// </summary>
    public bool AllowMembersToEdit { get; set; } = true;
}