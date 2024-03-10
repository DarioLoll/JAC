namespace JACService.Core.Contracts;

public interface IUser
{
    string Nickname { get; }
    bool LoggedIn { get; }
    void LogOut();
    void LogIn();
    
}