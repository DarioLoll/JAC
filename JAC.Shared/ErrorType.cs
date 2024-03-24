namespace JAC.Shared;

public enum ErrorType
{
    Unknown,
    InvalidPacket,
    UsernameTaken,
    AlreadyLoggedIn,
    NotLoggedIn,
    UserNotFound,
    CannotAddSelf
}