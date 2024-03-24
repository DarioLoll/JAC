using JAC.Shared;
using JAC.Shared.Channels;

namespace JACService.Core;

public static class ChannelHandler
{
    public static void SendMessage(this IChannel channel, string message)
    {
        
    }

    public static void SetGroupName(this GroupChannel channel, string name)
    {
        
    }

    public static void SetDescription(this GroupChannel channel, string description)
    {
        
    }

    public static void AddUser(this GroupChannel channel, IUser user)
    {
        
    }

    public static void PromoteToAdmin(this GroupChannel channel, IUser user)
    {
        
    }

    public static void RemoveUser(this GroupChannel channel, IUser user)
    {
        
    }
}