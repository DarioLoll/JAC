namespace JACService.Core.Commands;

public class SimpleCommand : TextCommand
{
    public SimpleCommand(string name) : this(name, () => true, "", name) {}
    
    public SimpleCommand(string name, Func<bool> execute, string description, string usage)
    {
        Name = name;
        Execute = execute;
        Description = description;
        Usage = usage;
    }
}