namespace JAC.Shared;

public class Message
{
    public IUser Sender { get; }
    public string Content { get; }
    public DateTime Time { get; }
    
    public Message(IUser sender, string content)
    {
        Sender = sender;
        Content = content;
        Time = DateTime.Now;
    }
}