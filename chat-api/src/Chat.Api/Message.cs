namespace Chat.Api;

internal class Message
{
    public Message(Guid id, string text, User sender, long timestamp)
    {
        Id = id;
        Text = text;
        Sender = sender;
        Timestamp = timestamp;
    }
    
    private Message() {}

    public Guid Id { get; }
    public string Text { get; }
    public User Sender { get; }
    public long Timestamp { get; }
}

internal record User(string Name, string Email);