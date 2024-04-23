using System.Text.Json;
using System.Text.Json.Serialization;

namespace JAC.Shared;

/// <summary>
/// Represents a message sent in a channel.
/// </summary>
public readonly struct Message
{
    /// <summary>
    /// The user that sent this message.
    /// </summary>
    public IUser Sender { get; }
    
    /// <summary>
    /// The content of the message.
    /// </summary>
    public string Content { get; }
    
    /// <summary>
    /// The time the message was sent.
    /// </summary>
    public DateTime TimeSent { get; }
    
    public Message(IUser sender, string content, DateTime timeSent = default)
    {
        Sender = sender;
        Content = content;
        TimeSent = timeSent == default ? DateTime.Now : timeSent;
    }
}