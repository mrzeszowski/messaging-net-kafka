namespace Chat.Messaging.Abstractions;

internal interface IEventFilter<in TEvent>
    where TEvent : IEvent
{
    public Task HandleAsync(TEvent message, Func<Task> next, CancellationToken cancellationToken);
}