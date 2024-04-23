using JAC.Shared;

namespace JACService.Core;

/// <summary>
/// The report of an action that was performed on the server (if it succeeded or not and what the error was).
/// </summary>
public class ActionReport
{
    /// <summary>
    /// If the action was successful.
    /// </summary>
    public bool Success { get; init; }
    
    /// <summary>
    /// If the action was not successful, the type of error that occurred.
    /// </summary>
    public ErrorType? Error { get; init; }
    
    /// <summary>
    /// A report indicating that the action was successful.
    /// </summary>
    public static ActionReport SuccessReport => new ActionReport { Success = true };
    
    /// <returns>An ActionReport indicating a failure and containing the given error as the error of the action</returns>
    public static ActionReport Failed(ErrorType error) => new() { Success = false, Error = error };
}