using JAC.Shared;

namespace JACService.Core;

/// <summary>
/// A result of an action that was performed on the server
/// </summary>
/// <typeparam name="T"></typeparam>
public class ActionResult<T> : ActionReport
{
    /// <summary>
    /// The result of the action.
    /// </summary>
    public T? Result { get; init; } = default;
    
    public static ActionResult<T> Succeeded(T result) => new() { Success = true, Result = result };
    public new static ActionResult<T> Failed(ErrorType error) => new() { Success = false, Error = error };
}