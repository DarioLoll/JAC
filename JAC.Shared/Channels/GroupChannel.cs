namespace JAC.Shared.Channels;

public class GroupChannel : IChannel
{
    public ulong Id { get; }
    public List<IUser> Users { get; }
    public List<Message> Messages { get; }
    public DateTime Created { get; }
    public string Name { get; set; }
    public string Description { get; set; }
    public List<IUser> Admins { get; }
    public GroupSettings Settings { get; }
    
    public GroupChannel(ulong id, string name, string description = "")
    {
        Id = id;
        Users = new List<IUser>();
        Messages = new List<Message>();
        Created = DateTime.Now;
        Admins = new List<IUser>();
        Settings = new GroupSettings();
        Name = name;
        Description = description;
    }
    
}