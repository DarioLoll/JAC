using MultiprotocolService.Service.Lib.RequestHandlers;

namespace MultiprotocolService.Service.Lib.Commands;

public class HelpCommand : TextCommand
{
    private readonly RequestHandlerFactory _requestHandlerFactory;

    public HelpCommand(RequestHandlerFactory requestHandlerFactory)
    {
        _requestHandlerFactory = requestHandlerFactory;
        Name = "help";
        Description = "Displays help for a command or all available commands.";
        Usage = "help [command]";
        Execute = () =>
        {
            string response;
            ITextCommand? requestedCommand = _requestHandlerFactory.Commands.FirstOrDefault(c => c.Name == Request);
            if (requestedCommand != null)
            {
                response = GetCommandInfo(requestedCommand);
            }
            else
            {
                response = "Available commands:\n";
                foreach (var command in _requestHandlerFactory.Commands)
                {
                    response += GetCommandInfo(command);
                }
            }
            Response = response;
            return true;
        };
    }
    
    private string GetCommandInfo(ITextCommand command)
    {
        return $"{command.Name}\n\t{command.Description}\n\tUsage: {command.Usage}\n";
    }
}