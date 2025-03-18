using System.Diagnostics;
using Chat.Messaging.Abstractions;
using Microsoft.Extensions.Logging;

namespace Chat.Messaging.Filters;

internal sealed class ObservabilityFilter<TEvent>(ILogger<ObservabilityFilter<TEvent>> logger) : IEventFilter<TEvent>
    where TEvent : IEvent
{
    public async Task HandleAsync(TEvent message, Func<Task> next, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
     
        await next();

        stopwatch.Stop();
        logger.LogInformation($"{typeof(TEvent)} handled in {stopwatch.ElapsedMilliseconds}ms");
    }
}