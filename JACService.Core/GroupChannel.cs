using JAC.Shared;
using JAC.Shared.Channels;

namespace JACService.Core;

public class GroupChannel : BaseChannel, IGroupChannel
{
    public string Name { get; set; }
    public string Description { get; set; }
    public List<IUser> Admins { get; }
    public GroupSettings Settings { get; }
    
    public GroupChannel(ulong id, IUser creator, string name, string description = "") : base(id)
    {
        Name = name;
        Description = description;
        Admins = new List<IUser> { creator };
        Settings = new GroupSettings();
    }
    
    public ActionReport AddUser(IUser user, IUser adder)
    {
        if(Users.Contains(user))
            return ActionReport.Failed(ErrorType.UserAlreadyInChannel);
        bool isAdderInChannel = Users.Contains(adder);
        bool isAdderAdmin = Admins.Contains(adder);
        bool areMembersAllowedToAdd = Settings.AllowMembersToAdd;
        if(!isAdderInChannel || (!isAdderAdmin && !areMembersAllowedToAdd))
            return ActionReport.Failed(ErrorType.InsufficientPermissions);
        AddUser(user);
        return ActionReport.SuccessReport;
    }

    public override ActionReport RemoveUser(IUser user, IUser remover)
    {
        if(!Users.Contains(user))
            return ActionReport.Failed(ErrorType.UserNotInChannel);
        bool isRemoverAdmin = Admins.Contains(remover);
        bool isRemoverInChannel = Users.Contains(remover);
        bool isUserRemover = user == remover;
        if((!isRemoverAdmin || !isRemoverInChannel) && !isUserRemover)
            return ActionReport.Failed(ErrorType.InsufficientPermissions);
        RemoveUser(user);
        return ActionReport.SuccessReport;
    }

    public override ActionReport SendMessage(IUser sender, string content)
    {
        bool isSenderAdmin = Admins.Contains(sender);
        if(Settings.ReadOnlyForMembers && !isSenderAdmin)
            return ActionReport.Failed(ErrorType.InsufficientPermissions);
        return base.SendMessage(sender, content);
    }
    
    public ActionReport ChangeName(IUser changer, string newName)
    {
        bool isChangerAdmin = Admins.Contains(changer);
        bool isChangerInChannel = Users.Contains(changer);
        bool areMembersAllowedToChangeName = Settings.AllowMembersToChangeName;
        if(!isChangerInChannel || (!isChangerAdmin && !areMembersAllowedToChangeName))
            return ActionReport.Failed(ErrorType.InsufficientPermissions);
        Name = newName;
        return ActionReport.SuccessReport;
    }
    
    public ActionReport ChangeDescription(IUser changer, string newDescription)
    {
        bool isChangerAdmin = Admins.Contains(changer);
        bool isChangerInChannel = Users.Contains(changer);
        bool areMembersAllowedToChangeDescription = Settings.AllowMembersToChangeDescription;
        if(!isChangerInChannel || (!isChangerAdmin && !areMembersAllowedToChangeDescription))
            return ActionReport.Failed(ErrorType.InsufficientPermissions);
        Description = newDescription;
        return ActionReport.SuccessReport;
    }
    
    public ActionReport ChangeUserRank(IUser user, IUser changer)
    {
        if(!Users.Contains(user))
            return ActionReport.Failed(ErrorType.UserNotInChannel);
        if(!Admins.Contains(changer))
            return ActionReport.Failed(ErrorType.InsufficientPermissions);
        if(Admins.Contains(user))
            Admins.Remove(user);
        else Admins.Add(user);
        return ActionReport.SuccessReport;
    }
    
    public static ActionResult<GroupChannel> CreateGroupChannel(ulong id, IUser creator, string name, string description = "")
    {
        var channel = new GroupChannel(id, creator, name, description);
        channel.AddUser(creator);
        channel.Admins.Add(creator);
        OnChannelCreated(channel);
        return ActionResult<GroupChannel>.Succeeded(channel);
    }
}