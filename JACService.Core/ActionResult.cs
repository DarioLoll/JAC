using JAC.Shared;

namespace JACService.Core;

/// <summary>
/// The result of an action that was performed on the server.
/// </summary>
public class ActionResult
{
    /// <summary>
    /// If the action was successful.
    /// </summary>
    public bool Success { get; init; }
    
    /// <summary>
    /// If the action was not successful, the type of error that occurred.
    /// </summary>
    public ErrorType? Error { get; init; }
    
    public static ActionResult SuccessResult => new ActionResult { Success = true };
    public static ActionResult UnknownError => new ActionResult { Success = false, Error = ErrorType.Unknown };
    public static ActionResult UserNotFound => new ActionResult { Success = false, Error = ErrorType.UserNotFound };
    public static ActionResult ChannelNotFound => new ActionResult { Success = false, Error = ErrorType.ChannelNotFound };
}