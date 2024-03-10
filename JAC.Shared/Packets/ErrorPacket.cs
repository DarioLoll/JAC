using System.Runtime.Serialization;
using System.Text.Json;

namespace JAC.Shared.Packets;

public class ErrorPacket : PacketBase
{
    public ErrorPacket(ErrorType errorType)
    {
        ErrorType = errorType;
        ErrorMessage = GenerateErrorMessage(errorType);
    }

    public string ErrorMessage { get; }
    public ErrorType ErrorType { get; }

    private static string GenerateErrorMessage(ErrorType errorType)
    {
        return errorType switch
        {
            ErrorType.Unknown => "An unknown error occurred",
            ErrorType.UsernameTaken => "The username is already taken",
            ErrorType.InvalidPacket => "The packet is invalid",
            ErrorType.AlreadyLoggedIn => "You are already logged in",
            _ => throw new ArgumentOutOfRangeException(nameof(errorType), errorType, null)
        };
    }

    public override string ToJson() => JsonSerializer.Serialize(this);
}
