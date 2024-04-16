using System.Text.Json.Serialization;
using JAC.Shared;

namespace JACService.Core;

public class BaseUser : IUser
{
    /// <summary>
    /// <inheritdoc cref="IUser.Nickname"/>
    /// </summary>
    public string Nickname { get; }

    [JsonInclude] private List<ulong> _channels = new();
    /// <summary>
    /// <inheritdoc cref="IUser.Channels"/>
    /// </summary>
    public IEnumerable<ulong> Channels => _channels;
    
    /// <summary>
    /// <inheritdoc cref="IUser.IsOnline"/>
    /// </summary>
    [JsonIgnore] public bool IsOnline { get; set; }
    
    public event Action<ulong>? JoinedChannel;
    public event Action<ulong>? LeftChannel;

    public BaseUser(string nickname)
    {
        Nickname = nickname;
    }

    public void JoinChannel(ulong channelId)
    {
        _channels.Add(channelId);
        OnJoinedChannel(channelId);
    }
    
    public void LeaveChannel(ulong channelId)
    {
        _channels.Remove(channelId);
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