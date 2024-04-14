using JAC.Shared.Channels;

namespace JACService.Core;

/// <summary>
/// Responsible for notifying clients of relevant events 
/// <example>When a user joins a channel,
/// the other users in that channel should be notified of it so they can update their UIs</example>
/// </summary>
public class EventNotifier
{
    public static EventNotifier Instance { get; } = new();

    private EventNotifier()
    {
        
    }
    
}