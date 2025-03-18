using Chat.Messaging.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Chat.Messaging.Filters;

internal sealed class TransactionFilter<TEvent>(DbContext dbContext) : IEventFilter<TEvent>
    where TEvent : IEvent
{
    public async Task HandleAsync(TEvent message, Func<Task> next, CancellationToken cancellationToken)
    {
        await next();
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}