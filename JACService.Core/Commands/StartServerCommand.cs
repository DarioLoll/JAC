using System.Net;

namespace MultiprotocolService.Service.Lib.Commands;

public class StartServerCommand : TextCommand
{
    private Server _server;

    public StartServerCommand(Server server)
    {
        _server = server;
        Name = "start";
        Description = "Starts the server on the specified port and ip address (default is loopback)";
        Usage = "start <port> [ip]";
        Execute = () =>
        {
            if (string.IsNullOrEmpty(Request)) return false;
            string[] splitRequest = Request.Split(' ');
            if (!ushort.TryParse(splitRequest[0], out var port)) return false;
            IPAddress ip = splitRequest.Length > 1 && IPAddress.TryParse(splitRequest[1], out var parsedIp) 
                ? parsedIp : IPAddress.Loopback;
            return server.Start(ip, port);
        };
    }
}