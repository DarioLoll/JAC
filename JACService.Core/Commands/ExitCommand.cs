namespace MultiprotocolService.Service.Lib.Commands;

public class ExitCommand : TextCommand
{
    private readonly Session _session;
    public ExitCommand(Session session)
    {
        _session = session;
        Name = "exit";
        Description = "Closes the session";
        Usage = "exit";
        Execute = () =>
        {
            Response = "Disconnecting from server...";
            _session.Close();
            return true;
        };
    }
    
}