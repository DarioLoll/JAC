using JAC.Shared;

namespace JACService.Core;

public static class UserHandler
{
    public static void SendMessage(this IUser user, ulong channelId, string message)
    {
        if (user.CanSendMessage(channelId))
        {
            // Send message
        }
    }

    public static bool CanSendMessage(this IUser user, ulong channelId)
    {
        return true;
    }
}