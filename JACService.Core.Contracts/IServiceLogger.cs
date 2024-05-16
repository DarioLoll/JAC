namespace JACService.Core.Contracts;

/// <summary>
/// A service that logs information and errors
/// </summary>
public interface IServiceLogger
{
    /// <summary>
    /// Logs the given message as information
    /// </summary>
    /// <param name="message">The message to log</param>
    /// <returns>A task that represents the asynchronous operation of writing to a file</returns>
    Task LogServiceInfoAsync(string message);
    
    /// <summary>
    /// Logs the given message as an error
    /// </summary>
    /// <param name="message">The message to log</param>
    /// <returns>A task that represents the asynchronous operation of writing to a file</returns>
    Task LogServiceErrorAsync(string message);
    
    /// <summary>
    /// Logs the given message as request information
    /// </summary>
    /// <param name="message">The message to log</param>
    /// <returns>A task that represents the asynchronous operation of writing to a file</returns>
    Task LogRequestInfoAsync(string message);
}