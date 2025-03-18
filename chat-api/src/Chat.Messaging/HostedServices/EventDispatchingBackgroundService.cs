using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Chat.Messaging.HostedServices;

internal class EventDispatchingBackgroundService(ILogger<EventDispatchingBackgroundService> logger, ConsumerPool consumerPool) 
    : BackgroundService
{
    private const string ConsumerGroupId = "2451c55d-08a3-4631-a490-5035bc8b8279.chat.api"; // todo make it unique
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();
        logger.LogInformation("Event dispatching is starting. Consumer Group ID: {0}", ConsumerGroupId);

        var subscriptions = new[]
        {
            new Subscription { Topics = ["chat.events"], TasksCount = 3},
        };

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await consumerPool.AdjustConsumers(ConsumerGroupId, subscriptions, stoppingToken);
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                logger.LogWarning("Event dispatching service cancelled.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while forwarding events.");
            }
        }

        logger.LogInformation("Event dispatching is stopping. Consumer Group ID: {0}", ConsumerGroupId);
    }
}