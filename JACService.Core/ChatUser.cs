using System.Text.Json.Serialization;
using JAC.Shared;

namespace JACService.Core;

/// <summary>
/// <inheritdoc cref="IUser"/> from the server's perspective.
/// </summary>
public class ChatUser : IUser
{
    
    /// <summary>
    /// <inheritdoc cref="IUser.Nickname"/>
    /// </summary>
    public required string Nickname { get; init; }  = string.Empty;
    
    /// <summary>
    /// <inheritdoc cref="IUser.LastSeen"/>
    /// </summary>
    public DateTime LastSeen { get; set; } = DateTime.MinValue;

    /// <summary>
    /// List of ids of channels the user is a member of.
    /// </summary>
    public IList<ulong> Channels { get; init; } = new List<ulong>();

    /// <summary>
    /// If the user is currently online.
    /// </summary>
    [JsonIgnore] public bool IsOnline { get; set; }
    
    /// <summary>
    /// Occurs when the user joins a channel.
    /// </summary>
    public event Action<ulong>? JoinedChannel;
    /// <summary>
    /// Occurs when the user leaves a channel.
    /// </summary>
    public event Action<ulong>? LeftChannel;
    
    /// <summary>
    /// Creates a ChatUser (server-side) from a <see cref="UserProfile"/>
    /// (a model for a user that is used to transfer data between the server and the client)
    /// by copying the properties of the model.
    /// </summary>
    /// <returns>The created user (ChatUser)</returns>
    public static ChatUser CreateFromModel(IUser userModel)
    {
        return new ChatUser
        {
            Nickname = userModel.Nickname,
            LastSeen = userModel.LastSeen
        };
    }
    

    /// <summary>
    /// Adds a channel to the user's list of channels.
    /// </summary>
    public void JoinChannel(ulong channelId)
    {
        Channels.Add(channelId);
        OnJoinedChannel(channelId);
    }
    
    /// <summary>
    /// Removes a channel from the user's list of channels.
    /// </summary>
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