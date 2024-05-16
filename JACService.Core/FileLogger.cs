using System.Text.Json;
using JACService.Core.Contracts;

namespace JACService.Core;

/// <summary>
/// Logs service information, errors, and requests to a file.
/// </summary>
public class FileLogger : IServiceLogger
{
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

    private FileLoggerOption _option = FileLoggerOption.AllToOneFile;

    /// <summary>
    /// Which log information to write to which file.
    /// </summary>
    public FileLoggerOption Option
    {
        get => _option;
        set
        {
            _option = value;
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
    
    public Task LogServiceInfoAsync(string message) => LogToFileAsync(message, "ServiceLogs.txt");

    public Task LogServiceErrorAsync(string message)
    {
        if(Option is FileLoggerOption.SeparateErrorFile or FileLoggerOption.SeparateAllFiles)
            return LogToFileAsync(message, ErrorLogFileName);
        return LogToFileAsync(message, MainLogFileName);
    }

    public Task LogRequestInfoAsync(string message)
    {
        if(Option is FileLoggerOption.SeparateRequestFile or FileLoggerOption.SeparateAllFiles)
            return LogToFileAsync(message, RequestLogFileName);
        return LogToFileAsync(message, MainLogFileName);
    }
    
    private async Task LogToFileAsync(string message, string fileName)
    {
        string logPath = Path.Combine(PathToLogFile, fileName);
        string formattedMessage = FormatLogMessage(message);
        await File.AppendAllTextAsync(logPath, formattedMessage);
        Console.WriteLine(formattedMessage);
    }
    
    private string FormatLogMessage(string message) => $"{DateTime.Now:G}: {message}\n";

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
}

public enum FileLoggerOption
{
    AllToOneFile,
    SeparateErrorFile,
    SeparateRequestFile,
    SeparateAllFiles
}