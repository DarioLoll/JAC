namespace MultiprotocolService.Service.Lib.Commands;

public abstract class TextCommand : ITextCommand
{
    public string Name { get; protected init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Usage { get; init; } = string.Empty;
    public string Response { get; protected set; } = string.Empty;
    public string? Request { get; set; }
    public Func<bool> Execute { get; init; } = () => true;

    public static bool TryGetResponse(ITextCommand? command, out string response)
    {
        if (command == null)
        {
            response = "Error: Invalid command, type 'help' for a list of commands.";
            return false;
        }
        //If the command is not found, the result will be false and
        //the response will be "Invalid command, type 'help' for a list of commands."
        var executed = command.Execute();
        //If the command was executed but no response was set, the response will be
        //"Command executed successfully." or "Command failed to execute." depending on the result.
        response = string.IsNullOrEmpty(command.Response) || executed == false
            ? (executed ? "Command executed successfully." : "Error: Command failed to execute.") 
            : command.Response;
        return executed;
    }
}