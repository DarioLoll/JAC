using JAC.Shared;
using JAC.Shared.Channels;

namespace JACService.Core;

public class GroupChannel : IGroupChannel
{
    public ulong Id { get; }
    public List<IUser> Users { get; }
    public List<Message> Messages { get; }
    public DateTime Created { get; }
    public string Name { get; set; }
    public string Description { get; set; }
    public List<IUser> Admins { get; }
    public GroupSettings Settings { get; }
    
    public GroupChannel(ulong id, IUser creator, string name, string description = "")
    {
        Id = id;
        Users = new List<IUser> { creator };
        Messages = new List<Message>();
        Created = DateTime.Now;
        Name = name;
        Description = description;
        Admins = new List<IUser> { creator };
        Settings = new GroupSettings();
    }
}