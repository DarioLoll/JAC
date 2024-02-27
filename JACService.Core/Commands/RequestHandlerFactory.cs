using MultiprotocolService.Service.Lib.Commands;

namespace MultiprotocolService.Service.Lib.RequestHandlers;

public class RequestHandlerFactory
{
    private readonly List<ITextCommand> _commands;
    public IEnumerable<ITextCommand> Commands => _commands;

    public RequestHandlerFactory(List<ITextCommand> commands)
    {
        _commands = commands;
    }
    
    public void AddCommand(ITextCommand command) => _commands.Add(command);
    
    public void AddHelpCommand() => _commands.Add(new HelpCommand(this));

    public ITextCommand? GetTextCommand(string request)
    {
        foreach (var command in _commands)
        {
            if (request.StartsWith(command.Name))
            {
                command.Request = request.Substring(command.Name.Length).Trim();
                return command;
            }
        }

        return null;
    }
}