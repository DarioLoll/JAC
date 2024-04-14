using JAC.Shared;

namespace JACService.Core;

/// <summary>
/// The report of an action that was performed on the server (if it succeeded or not).
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
    
    public static ActionReport SuccessReport => new ActionReport { Success = true };
    
    public static ActionReport Failed(ErrorType error) => new() { Success = false, Error = error };
    public static ActionReport UnknownError => new ActionReport { Success = false, Error = ErrorType.Unknown };
    public static ActionReport UserNotFound => new ActionReport { Success = false, Error = ErrorType.UserNotFound };
    public static ActionReport ChannelNotFound => new ActionReport { Success = false, Error = ErrorType.ChannelNotFound };
}