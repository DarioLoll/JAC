namespace MultiprotocolService.Service.Lib.Commands;

public interface ITextCommand
{
    string Name { get; }
    string Description { get; }
    string Usage { get; }
    string Response { get; }
    string? Request { get; set; }
    Func<bool> Execute { get; }
}