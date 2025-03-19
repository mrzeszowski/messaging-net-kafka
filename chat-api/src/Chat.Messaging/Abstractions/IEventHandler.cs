namespace Chat.Messaging.Abstractions;

public interface IEventHandler<in TEvent>
    where TEvent : IEvent
{
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken);
}