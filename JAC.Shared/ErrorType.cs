namespace JAC.Shared;

/// <summary>
/// The type of error that occurred on the server that is being sent to the client.
/// </summary>
public enum ErrorType
{
    Unknown,
    InvalidPacket,
    UsernameTaken,
    AlreadyLoggedIn,
    NotLoggedIn,
    UserNotFound,
    CannotAddSelf,
    ChannelNotFound,
    UserAlreadyInChannel,
    InsufficientPermissions,
    UserNotInChannel,
    ChannelAlreadyExists
}