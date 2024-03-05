using System.Runtime.Serialization;
using System.Text.Json;

namespace JACService.Core;

[DataContract]
public class FileLogger : IServiceLogger
{
    [DataMember]
    private bool _overwriteOnRestart;

    public bool OverwriteOnRestart
    {
        get => _overwriteOnRestart;
        set
        {
            _overwriteOnRestart = value;
            SaveConfig();
        }
    }

    [DataMember] 
    private FileLoggerOption _option = FileLoggerOption.AllToOneFile;

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
}

public enum FileLoggerOption
{
    AllToOneFile,
    SeparateErrorFile,
    SeparateRequestFile,
    SeparateAllFiles
}