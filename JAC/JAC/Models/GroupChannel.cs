using System;
using System.Collections.Generic;
using System.Linq;
using JAC.Shared;
using JAC.Shared.Channels;

namespace JAC.Models;

/// <summary>
/// <inheritdoc cref="IGroupChannel"/> - from a client's perspective
/// </summary>
public class GroupChannel : BaseChannel, IGroupChannel
{
    /// <summary>
    /// <inheritdoc cref="IGroupChannel.Name"/>
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// <inheritdoc cref="IGroupChannel.Description"/>
    /// </summary>
    public string Description { get; set; } = string.Empty;
    /// <summary>
    /// <inheritdoc cref="IGroupChannel.Admins"/>
    /// </summary>
    public IList<IUser> Admins { get; init; } = new List<IUser>();
    /// <summary>
    /// <inheritdoc cref="IGroupChannel.Settings"/>
    /// </summary>
    public GroupSettings Settings { get; init; }
    
    public GroupChannel(ulong id, DateTime created, string name) : base(id, created)
    {
        Name = name;
    }
    
    /// <summary>
    /// Creates a GroupChannel (client-side) from a <see cref="GroupChannelModel"/>
    /// (a model for a group channel that is used to transfer data between the server and the client)
    /// by copying the properties of the model.
    /// </summary>
    public GroupChannel(IGroupChannel model) : base(model)
    {
        Name = model.Name;
        Description = model.Description;
        Admins = model.Admins.Select(admin => new User(admin)).Cast<IUser>().ToList();
        Settings = model.Settings;
    }
    
}