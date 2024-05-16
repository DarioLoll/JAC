
using JACService.Core.Contracts;

namespace JACService.Core;

/// <summary>
/// A service that logs information and errors to the console
/// </summary>
public class ServiceConsoleLogger : IServiceLogger
{
    public Task LogServiceInfoAsync(string message)
    {
        Log(message, ConsoleColor.Green);
        return Task.CompletedTask;
    }

    public Task LogServiceErrorAsync(string message)
    {
        Log(message, ConsoleColor.Red);
        return Task.CompletedTask;
    }

    public Task LogRequestInfoAsync(string message)
    {
        Log(message, ConsoleColor.Yellow);
        return Task.CompletedTask;
    }

    private void Log(string message, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.WriteLine($"\r[{DateTime.Now.ToLongTimeString()}]: {message}");
        Console.ResetColor();
    }
}