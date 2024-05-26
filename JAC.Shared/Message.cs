namespace JAC.Shared;

/// <summary>
/// Represents a message sent in a channel.
/// </summary>
public class Message : IComparable<Message>
{
    /// <summary>
    /// The id of the user who sent this message.
    /// </summary>
    public string SenderName { get; set; }
    
    /// <summary>
    /// The content of the message.
    /// </summary>
    public string Content { get; set; }
    
    /// <summary>
    /// The time the message was sent.
    /// </summary>
    public DateTime TimeSent { get; }
    
    public Message(string senderName, string content, DateTime timeSent = default)
    {
        SenderName = senderName;
        Content = content;
        TimeSent = timeSent == default ? DateTime.Now : timeSent;
    }

    public Message()
    {
        SenderName = string.Empty;
        Content = string.Empty;
        TimeSent = DateTime.Now;
    }

    /// <summary>
    /// Compares the value of this instance to a specified <see cref="Message"/> value
    /// and returns an integer that indicates whether this message was sent/created earlier than,
    /// at the same as, or later than the specified <see cref="Message"/> value.
    /// </summary>
    /// <param name="other">The object to compare to the current instance.</param>
    public int CompareTo(Message? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (ReferenceEquals(null, other)) return 1;
        return TimeSent.CompareTo(other.TimeSent);
    }
}