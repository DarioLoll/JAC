using JACService.Core.Logging;

namespace JACService.Core;

/// <summary>
/// A service that logs information and errors
/// </summary>
public interface IServiceLogger
{
    List<LogEntry> Entries { get; }
    
    event Action<LogEntry>? LogEntryAdded;
    
    bool DetailedLogging { get; set; }
    
    /// <summary>
    /// Logs the given message as information
    /// </summary>
    /// <param name="type">The type of log</param>
    /// <param name="message">The message to log</param>
    /// <param name="isDetail">If the logged message is important or just a detail</param>
    /// <returns>A task that represents the asynchronous operation of writing to a file</returns>
    Task LogAsync(LogType type, string message, bool isDetail = false);
    
}