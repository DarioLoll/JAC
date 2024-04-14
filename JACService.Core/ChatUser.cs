using JAC.Shared;
using JAC.Shared.Channels;
using JACService.Core.Contracts;

namespace JACService.Core;

public class ChatUser : IUser
{
    /// <summary>
    /// <inheritdoc cref="IUser.Nickname"/>
    /// </summary>
    public string Nickname { get; }
    
    /// <summary>
    /// <inheritdoc cref="IUser.Channels"/>
    /// </summary>
    public List<ulong> Channels { get; }
    
    /// <summary>
    /// <inheritdoc cref="IUser.IsOnline"/>
    /// </summary>
    public bool IsOnline { get; set; }
    

    public ChatUser(string nickname, List<ulong>? channels = null)
    {
        Nickname = nickname;
        Channels = channels ?? new List<ulong>();
    }
}