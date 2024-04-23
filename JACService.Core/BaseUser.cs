using JAC.Shared;

namespace JACService.Core;

public class BaseUser : IUser
{
    
    public required string Nickname { get; init; }  = string.Empty;
    public IList<ulong> Channels { get; init; } = new List<ulong>();

    /// <summary>
    /// If the user is currently online.
    /// </summary>
    public bool IsOnline { get; set; }
    
    public event Action<ulong>? JoinedChannel;
    public event Action<ulong>? LeftChannel;
    
    public static BaseUser CreateFromModel(IUser userModel)
    {
        return new BaseUser
        {
            Nickname = userModel.Nickname,
            Channels = userModel.Channels.ToList()
        };
    }

    public static List<IUser> CreateFromModels(IEnumerable<IUser> userModels)
    {
        return userModels.Select(user => CreateFromModel(user)).Cast<IUser>().ToList();
    }

    public void JoinChannel(ulong channelId)
    {
        Channels.Add(channelId);
        OnJoinedChannel(channelId);
    }
    
    public void LeaveChannel(ulong channelId)
    {
        Channels.Remove(channelId);
        OnLeftChannel(channelId);
    }

    protected virtual void OnJoinedChannel(ulong channelId)
    {
        JoinedChannel?.Invoke(channelId);
    }

    protected virtual void OnLeftChannel(ulong channelId)
    {
        LeftChannel?.Invoke(channelId);
    }
}