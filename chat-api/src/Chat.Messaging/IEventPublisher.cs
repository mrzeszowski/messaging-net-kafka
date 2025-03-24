using System.Text;
using System.Text.Json;
using Chat.Messaging.Abstractions;
using Chat.Messaging.Ef;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Chat.Messaging;

public interface IEventPublisher
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IEvent;
}

internal class EventPublisher(DbContext dbContext) : IEventPublisher
{
    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IEvent
    {
        var entity = new EventOutbox(
            localOffset: 0,
            id: Guid.NewGuid(),
            type: typeof(TEvent).FullName!,
            payload: Encoding.UTF8.GetBytes(JsonSerializer.Serialize(@event)),
            timestamp: SystemClock.Instance.GetCurrentInstant().ToUnixTimeTicks(),
            topic: "chat.events", // todo this needs to be managed
            partitionKey: Encoding.UTF8.GetBytes(@event.Id.ToString()));

        await dbContext.Set<EventOutbox>().AddAsync(entity, cancellationToken);
    }
}