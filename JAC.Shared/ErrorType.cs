namespace JAC.Shared;

/// <summary>
/// The type of error that occurred on the server that is being sent to the client.
/// </summary>
public enum ErrorType
{
    UnknownError,
    UsernameTaken,
    AlreadyLoggedIn,
    NotLoggedIn,
    UserNotFound,
    ChannelNotFound,
    UserAlreadyInChannel,
    InsufficientPermissions,
    UserNotInChannel,
}

public static class ErrorEnumToStringConverter
{
    public static string GetErrorMessage(this ErrorType errorType)
    {
        string errorString = errorType.ToString();
        List<string> words = new();
        foreach (char character in errorString)
        {
            if (char.IsUpper(character))
            {
                words.Add(character.ToString());
            }
            else
            {
                words[^1] += character;
            }
        }
        return string.Join(" ", words);
    }
}