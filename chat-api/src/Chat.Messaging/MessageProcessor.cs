using System.Reflection;
using System.Text;
using System.Text.Json;
using Chat.Messaging.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Chat.Messaging;

internal class MessageProcessor(IServiceProvider serviceProvider, ILogger<MessageProcessor> logger, TypeRepository typeRepository)
{
    public async Task ProcessAsync(ConsumedMessage message, CancellationToken cancellationToken = default)
    {
        try
        {
            var type = typeRepository.Types.Single(x => x.FullName == message.Headers["message.type"]);
            var @event = (IEvent)JsonSerializer.Deserialize(Encoding.UTF8.GetString(message.Payload), type)!;

            await using var scope = serviceProvider.CreateAsyncScope();

            var pipelineType = typeof(EventPipelineFactory<>).MakeGenericType(type);
            var pipelineFactory = Activator.CreateInstance(pipelineType, scope.ServiceProvider) as IEventPipelineFactory;
            
            var pipeline = pipelineFactory!.Create();
            await pipeline(@event, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            // build dead-letter
            // do not throw
        }
    }
}

internal class TypeRepository(Type assembly)
{
    public readonly Type[] Types = Assembly.GetAssembly(assembly)?.GetTypes() ?? Type.EmptyTypes;
}