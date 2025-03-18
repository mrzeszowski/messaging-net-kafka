namespace Chat.Messaging.Abstractions;

public interface IEvent
{
    Guid Id { get; }
};