using System.Runtime.Serialization;
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
            SaveConfig();
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
            SaveConfig();
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
    
    public void LogServiceInfo(string message) => LogToFile(message, "ServiceLogs.txt");

    public void LogServiceError(string message)
    {
        if(Option is FileLoggerOption.SeparateErrorFile or FileLoggerOption.SeparateAllFiles)
            LogToFile(message, ErrorLogFileName);
        else
            LogToFile(message, MainLogFileName);
    }

    public void LogRequestInfo(string message)
    {
        if(Option is FileLoggerOption.SeparateRequestFile or FileLoggerOption.SeparateAllFiles)
            LogToFile(message, RequestLogFileName);
        else
            LogToFile(message, MainLogFileName);
    }
    
    private void LogToFile(string message, string fileName)
    {
        string logPath = Path.Combine(PathToLogFile, fileName);
        File.AppendAllText(logPath, FormatLogMessage(message));
    }
    
    private string FormatLogMessage(string message) => $"{DateTime.Now:G}: {message}\n";
    
    public void ClearLogs()
    {
        File.WriteAllText(Path.Combine(PathToLogFile, MainLogFileName), string.Empty);
        File.WriteAllText(Path.Combine(PathToLogFile, ErrorLogFileName), string.Empty);
        File.WriteAllText(Path.Combine(PathToLogFile, RequestLogFileName), string.Empty);
    }

    private void SaveConfig()
    {
        string configPath = Path.Combine(PathToLogFile, ConfigFileName);
        string json = JsonSerializer.Serialize(this);
        File.WriteAllText(configPath, json);
    }

    public void LoadConfig()
    {
        string configPath = Path.Combine(PathToLogFile, ConfigFileName);
        if (!File.Exists(configPath)) return;
        string json = File.ReadAllText(configPath);
        var savedLogger = JsonSerializer.Deserialize<FileLogger>(json);
        if (savedLogger is null) return;
        OverwriteOnRestart = savedLogger.OverwriteOnRestart;
        Option = savedLogger.Option;
        if (OverwriteOnRestart) 
            ClearLogs();
    }
    
    /// <summary>
    /// Attempts to load a FileLogger from a config file. If the file does not exist, a new FileLogger is created.
    /// </summary>
    /// <param name="pathToLogFile">The path to the directory where the config file is stored, excluding the file name</param>
    /// <returns>A FileLogger configured as saved in the config file, or a new, default FileLogger if the config file was not found</returns>
    public static FileLogger LoadFromConfig(string pathToLogFile)
    {
        string configPath = Path.Combine(pathToLogFile, ConfigFileName);
        if (!File.Exists(configPath)) return new FileLogger(pathToLogFile);
        string json = File.ReadAllText(configPath);
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