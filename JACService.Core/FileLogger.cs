namespace JACService.Core;

public class FileLogger : IServiceLogger
{
    public bool OverwriteOnRestart { get; set; } = false;
    public FileLoggerOption Option { get; set; } = FileLoggerOption.AllToOneFile;
    
    private const string MainLogFileName = "ServiceLogs.txt";
    private const string ErrorLogFileName = "ServiceErrorLogs.txt";
    private const string RequestLogFileName = "RequestLogs.txt";
    
    /// <summary>
    /// Path to the log file excluding the file name
    /// </summary>
    public string PathToLogFile { get; set; }

    public FileLogger(string pathToLogFile)
    {
        PathToLogFile = pathToLogFile;
        if (OverwriteOnRestart) 
            ClearLogs();
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
        File.AppendAllText(logPath, message);
    }
    
    public void ClearLogs()
    {
        File.WriteAllText(Path.Combine(PathToLogFile, MainLogFileName), string.Empty);
        File.WriteAllText(Path.Combine(PathToLogFile, ErrorLogFileName), string.Empty);
        File.WriteAllText(Path.Combine(PathToLogFile, RequestLogFileName), string.Empty);
    }
}

public enum FileLoggerOption
{
    AllToOneFile,
    SeparateErrorFile,
    SeparateRequestFile,
    SeparateAllFiles
}