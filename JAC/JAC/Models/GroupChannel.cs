using System;
using System.Collections.Generic;
using System.Linq;
using JAC.Shared;
using JAC.Shared.Channels;

namespace JAC.Models;

public class GroupChannel : BaseChannel, IGroupChannel
{
    public string Name { get; set; }
    public string Description { get; set; } = string.Empty;
    public IList<IUser> Admins { get; init; } = new List<IUser>();
    public GroupSettings Settings { get; init; }
    
    public GroupChannel(ulong id, DateTime created, string name) : base(id, created)
    {
        Name = name;
    }
    
    public GroupChannel(IGroupChannel model) : base(model)
    {
        Name = model.Name;
        Description = model.Description;
        Admins = model.Admins.Select(admin => new User(admin)).Cast<IUser>().ToList();
        Settings = model.Settings;
    }
    
}