using JAC.Shared;
using JAC.Shared.Channels;

namespace JACService.Core;

public class GroupChannel : BaseChannel, IGroupChannel
{
    public required string Name { get; set; }
    public string Description { get; set; } = string.Empty;
    public IList<IUser> Admins { get; init; } = new List<IUser>();
    public GroupSettings Settings { get; init; } = new();
    
    public event Action<string>? NameChanged;
    public event Action<string>? DescriptionChanged;
    public event Action<BaseUser>? RankChanged;
    
    public ActionReport AddUser(BaseUser user, BaseUser adder)
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

    public override ActionReport RemoveUser(BaseUser user, BaseUser remover)
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

    public override ActionReport SendMessage(BaseUser sender, string content)
    {
        bool isSenderAdmin = Admins.Contains(sender);
        if(Settings.ReadOnlyForMembers && !isSenderAdmin)
            return ActionReport.Failed(ErrorType.InsufficientPermissions);
        return base.SendMessage(sender, content);
    }
    
    public ActionReport ChangeName(BaseUser changer, string newName)
    {
        bool isChangerAdmin = Admins.Contains(changer);
        bool isChangerInChannel = Users.Contains(changer);
        bool areMembersAllowedToChangeName = Settings.AllowMembersToChangeName;
        if(!isChangerInChannel || (!isChangerAdmin && !areMembersAllowedToChangeName))
            return ActionReport.Failed(ErrorType.InsufficientPermissions);
        Name = newName;
        OnNameChanged(newName);
        return ActionReport.SuccessReport;
    }
    
    public ActionReport ChangeDescription(BaseUser changer, string newDescription)
    {
        bool isChangerAdmin = Admins.Contains(changer);
        bool isChangerInChannel = Users.Contains(changer);
        bool areMembersAllowedToChangeDescription = Settings.AllowMembersToChangeDescription;
        if(!isChangerInChannel || (!isChangerAdmin && !areMembersAllowedToChangeDescription))
            return ActionReport.Failed(ErrorType.InsufficientPermissions);
        Description = newDescription;
        OnDescriptionChanged(newDescription);
        return ActionReport.SuccessReport;
    }
    
    public ActionReport ChangeUserRank(BaseUser user, BaseUser changer)
    {
        if(!Users.Contains(user))
            return ActionReport.Failed(ErrorType.UserNotInChannel);
        if(!Admins.Contains(changer))
            return ActionReport.Failed(ErrorType.InsufficientPermissions);
        if(Admins.Contains(user))
            Admins.Remove(user);
        else Admins.Add(user);
        OnRankChanged(user);
        return ActionReport.SuccessReport;
    }
    
    public static ActionResult<GroupChannel> CreateGroupChannel(ulong id, BaseUser creator, string name, 
        string description, DateTime created = default)
    {
        var channel = new GroupChannel
        {
            Id = id,
            Name = name,
            Description = description,
            Created = created == default ? DateTime.Now : created
        };
        channel.AddUser(creator);
        channel.Admins.Add(creator);
        OnChannelCreated(channel);
        return ActionResult<GroupChannel>.Succeeded(channel);
    }

    protected virtual void OnNameChanged(string newName)
    {
        NameChanged?.Invoke(newName);
    }

    protected virtual void OnDescriptionChanged(string obj)
    {
        DescriptionChanged?.Invoke(obj);
    }

    protected virtual void OnRankChanged(BaseUser user)
    {
        RankChanged?.Invoke(user);
    }
}