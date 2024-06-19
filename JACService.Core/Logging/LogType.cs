namespace JACService.Core.Logging;

public enum LogType
{
    Info,
    Warning,
    Error,
    Request
}

public static class LogTypeExtensions
{
    public static string ToFormattedString(this LogType type)
    {
        return type switch
        {
            LogType.Info => "",
            LogType.Warning => "WARNING",
            LogType.Error => "ERROR",
            LogType.Request => "REQUEST",
            _ => ""
        };
    }
}