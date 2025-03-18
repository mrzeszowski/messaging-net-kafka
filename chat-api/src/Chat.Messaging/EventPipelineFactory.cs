using Chat.Messaging.Abstractions;
using Chat.Messaging.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Chat.Messaging;

internal sealed class EventPipelineFactory<TEvent>(IServiceProvider serviceProvider) : IEventPipelineFactory
    where TEvent : IEvent
{
    private readonly IReadOnlyCollection<Type> _defaultFilters =
    [
        typeof(TransactionFilter<>),
        typeof(ObservabilityFilter<>),
    ];

    public EventPipeline<IEvent> Create()
    {
        var filters = _defaultFilters
            .Select(x => serviceProvider.GetRequiredService(x.MakeGenericType(typeof(TEvent))) as IEventFilter<TEvent>)
            .Select(x => x!)
            .ToArray();
  
        return (message, cancellationToken) =>
        {
            var pipeline = filters.Aggregate(
                seed: HandleEventAsync(),
                func: (next, filter) => (m, ct) => filter.HandleAsync((TEvent)m, () => next(m, ct), ct));
            
            return pipeline(message, cancellationToken);
        };
    }
    
    private EventPipeline<IEvent> HandleEventAsync()
    {
        var handlers = serviceProvider.GetServices<IEventHandler<TEvent>>();
        
        return async (message, cancellationToken) =>
        {
            foreach (var handler in handlers)
            {
                await handler.HandleAsync((TEvent)message, cancellationToken);
            }
        };
    }
}

internal interface IEventPipelineFactory
{
    EventPipeline<IEvent> Create();
}

internal delegate Task EventPipeline<in TMessage>(TMessage message, CancellationToken cancellationToken)
    where TMessage : IEvent;