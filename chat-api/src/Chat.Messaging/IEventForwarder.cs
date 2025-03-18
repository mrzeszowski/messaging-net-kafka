using Chat.Messaging.Ef;
using Microsoft.EntityFrameworkCore;

namespace Chat.Messaging;

public interface IEventForwarder
{
    Task<int> ForwardAsync(CancellationToken cancellationToken);
}

internal class EventForwarder(DbContext dbContext, IProducer producer) : IEventForwarder
{
    public async Task<int> ForwardAsync(CancellationToken cancellationToken)
    {
        var events = await dbContext.Set<EventOutbox>()
            .OrderBy(x => x.LocalOffset)
            .Take(100)
            .ToArrayAsync(cancellationToken);

        foreach (var @event in events)
        {
            await producer.ProduceAsync(new ProducerMessage(id: @event.Id.ToString(),
                    type: @event.Type,
                    partitionKey: @event.PartitionKey,
                    payload: @event.Payload,
                    topic: @event.Topic),
                cancellationToken);
        }
        
        await ForwardedAsync(events, cancellationToken);
        return events.Length;
    }

    private async Task ForwardedAsync(EventOutbox[] events, CancellationToken cancellationToken)
    {
        var eventLog = events
            .Select(
                x => new EventLog(
                    localOffset: x.LocalOffset,
                    id: x.Id,
                    type: x.Type,
                    payload: x.Payload,
                    timestamp: x.Timestamp,
                    x.Topic,
                    x.PartitionKey))
            .ToArray();

        await dbContext.Set<EventOutbox>().Where(model => events.Select(x => x.Id).Contains(model.Id)).ExecuteDeleteAsync(cancellationToken);
        await dbContext.Set<EventLog>().AddRangeAsync(eventLog, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}