using JAC.Shared;
using JACService.Core.Contracts;

namespace JACService.Core;

public class ChatUser : IUser
{
    public string Nickname { get; }
    public List<ulong> Channels { get; }
    
    public bool IsOnline { get; set; }


    public ChatUser(string nickname, List<ulong>? channels = null)
    {
        Nickname = nickname;
        Channels = channels ?? new List<ulong>();
    }
    
}