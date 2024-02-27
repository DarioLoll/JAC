namespace MultiprotocolService.Service.Lib.Commands;

public class ServerStatusCommand : TextCommand
{
    private readonly Server _server;
    
    public ServerStatusCommand(Server server)
    {
        _server = server;
        Name = "status";
        Description = "Checks the status of the server";
        Usage = "status";
        Execute = () =>
        {
            Response = _server.IsOnline ? "The server is online." : "The server is offline.";
            Response += "\nConnected clients: " + _server.ClientCount;
            return true;
        };
    }
}