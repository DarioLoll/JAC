using JACService.Core.Logging;

namespace JACService.Core;

public class LogEntry
{
    public required string Content { get; set; }

    public DateTime TimeStamp { get; set; } = DateTime.Now;

    public required LogType Type { get; set; }

    public bool IsDetail { get; set; }
    
    public string Formatted => ToString();
    

    public override string ToString()
    {
        return $"[{TimeStamp:HH:mm:ss}] {Type.ToFormattedString()} {Content}";
    }
}