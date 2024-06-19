using System.Text.Json;
using JACService.Core.Logging;

namespace JACService.Core;

/// <summary>
/// Logs service information, errors, and requests to a file.
/// </summary>
public class FileLogger : IServiceLogger
{
    public List<LogEntry> Entries { get; } = new();
    public event Action<LogEntry>? LogEntryAdded;
    
    public bool DetailedLogging { get; set; } = true;
    
    private bool _overwriteOnRestart;

    /// <summary>
    /// Whether to start with a clean log file on service restart.
    /// </summary>
    public bool OverwriteOnRestart
    {
        get => _overwriteOnRestart;
        set
        {
            _overwriteOnRestart = value;
            _ = Task.Run(SaveConfigAsync);
        }
    }

    private const string MainLogFileName = "ServiceLogs.txt";
    private const string ErrorLogFileName = "ServiceErrorLogs.txt";
    private const string RequestLogFileName = "RequestLogs.txt";
    private const string ConfigFileName = "config.json";
    
    /// <summary>
    /// Path to the log file excluding the file name
    /// </summary>
    public string PathToLogFile { get; set; }


    /// <param name="pathToLogFile">Path to the log file excluding the file name</param>
    public FileLogger(string pathToLogFile)
    {
        PathToLogFile = pathToLogFile;
    }
    
    public async Task LogAsync(LogType type, string message, bool isDetail = false)
    {
        string fileName = type switch
        {
            LogType.Info => MainLogFileName,
            LogType.Warning => ErrorLogFileName,
            LogType.Error => ErrorLogFileName,
            LogType.Request => RequestLogFileName,
            _ => MainLogFileName
        };

        var entry = new LogEntry
        {
            Content = message,
            Type = type,
            IsDetail = isDetail
        };
        OnLogEntryAdded(entry);
        if (isDetail && !DetailedLogging)
        {
            return;
        }
        await LogToFileAsync(entry, fileName);
    }
    
    private async Task LogToFileAsync(LogEntry entry, string fileName)
    {
        string logPath = Path.Combine(PathToLogFile, fileName);
        await File.AppendAllTextAsync(logPath, entry.Formatted + Environment.NewLine);
    }
    
    private async Task SaveConfigAsync()
    {
        string configPath = Path.Combine(PathToLogFile, ConfigFileName);
        string json = JsonSerializer.Serialize(this);
        await File.WriteAllTextAsync(configPath, json);
    }
    
    /// <summary>
    /// Attempts to load a FileLogger from a config file. If the file does not exist, a new FileLogger is created.
    /// </summary>
    /// <param name="pathToLogFile">The path to the directory where the config file is stored, excluding the file name</param>
    /// <returns>A FileLogger configured as saved in the config file, or a new, default FileLogger if the config file was not found</returns>
    public static async Task<FileLogger> LoadFromConfigAsync(string pathToLogFile)
    {
        string configPath = Path.Combine(pathToLogFile, ConfigFileName);
        if (!File.Exists(configPath)) return new FileLogger(pathToLogFile);
        string json = await File.ReadAllTextAsync(configPath);
        return JsonSerializer.Deserialize<FileLogger>(json) ?? new FileLogger(pathToLogFile);
    }

    protected virtual void OnLogEntryAdded(LogEntry entry)
    {
        Entries.Add(entry);
        LogEntryAdded?.Invoke(entry);
    }
}
