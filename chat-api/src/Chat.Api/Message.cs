using System.Globalization;
using NodaTime;

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
    
    public string CreatedAt => Instant.FromUnixTimeTicks(Timestamp).ToDateTimeUtc().ToString("dd MMMM yyyy HH:mm", CultureInfo.CurrentCulture);
}

internal record User(string Name, string Email);