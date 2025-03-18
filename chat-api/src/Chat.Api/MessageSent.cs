using Chat.Messaging.Abstractions;

namespace Chat.Api;

public sealed class MessageSent(Guid id, MessageSent.MessageDto data) : IEvent
{
    public Guid Id { get; init; } = id;
    public MessageDto Data { get; init; } = data;
    
    public sealed record MessageDto(string Text, UserDto Sender, long Timestamp);
    public sealed record UserDto(string Name, string Email);
}

