namespace MultiprotocolService.Service.Lib;

public class ServiceConsoleLogger : IServiceLogger
{
    public void LogServiceInfo(string message) => Log(message, ConsoleColor.Green);

    public void LogServiceError(string message) => Log(message, ConsoleColor.Red);

    public void LogRequestInfo(string message) => Log(message, ConsoleColor.Yellow);
    
    private void Log(string message, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.WriteLine($"\r[{DateTime.Now.ToLongTimeString()}]: {message}");
        Console.ResetColor();
    }
}