
namespace JACService.Core.Logging;

/// <summary>
/// A service that logs information and errors to the console
/// </summary>
public class ServiceConsoleLogger : IServiceLogger
{
    public List<LogEntry> Entries { get; } = new();
    public event Action<LogEntry>? LogEntryAdded;
    public bool DetailedLogging { get; set; }

    public ServiceConsoleLogger(bool logDetails)
    {
        DetailedLogging = logDetails;
    }
    
    public Task LogAsync(LogType type, string message, bool isDetail = false)
    {
        if (isDetail && !DetailedLogging)
        {
            return Task.CompletedTask;
        }

        var color = type switch
        {
            LogType.Info => ConsoleColor.Gray,
            LogType.Warning => ConsoleColor.Yellow,
            LogType.Error => ConsoleColor.Red,
            LogType.Request => ConsoleColor.Cyan,
            _ => ConsoleColor.Gray
        };

        Console.ForegroundColor = color;
        var logEntry = new LogEntry
        {
            Content = message,
            Type = type,
            IsDetail = isDetail
        };
        Console.WriteLine(logEntry.Formatted);
        OnLogEntryAdded(logEntry);
        Console.ResetColor();
        return Task.CompletedTask;
    }

    protected virtual void OnLogEntryAdded(LogEntry obj)
    {
        Entries.Add(obj);
        LogEntryAdded?.Invoke(obj);
    }
}