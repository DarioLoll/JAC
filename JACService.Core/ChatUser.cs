using JACService.Core.Contracts;

namespace JACService.Core;

public class ChatUser : IUser
{
    public string Nickname { get; }

    public bool LoggedIn { get; private set; }
    
    public ChatUser(string nickname)
    {
        Nickname = nickname;
    }
    
    public void LogOut()
    {
        LoggedIn = false;
    }
    
    public void LogIn()
    {
        LoggedIn = true;
    }
}