using Medallion.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Chat.Messaging.HostedServices;

internal sealed class EventForwarderBackgroundService(
    ILogger<EventForwarderBackgroundService> logger,
    [FromKeyedServices(nameof(EventForwarderBackgroundService))]
    IDistributedLockProvider lockProvider,
    IServiceProvider serviceProvider)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();
        logger.LogInformation("Event forwarding service started.");
        
        var @lock = lockProvider.CreateLock("19a54c7c-bd20-43b2-a9a3-12e5c449c37b"); // todo make it unique 
        await using (_ = await @lock.AcquireAsync(cancellationToken: stoppingToken))
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await using var scope = serviceProvider.CreateAsyncScope();
                    var forwarder = scope.ServiceProvider.GetRequiredService<IEventForwarder>();
                    var eventsCount = await forwarder.ForwardAsync(stoppingToken);

                    await DelayForwarding(eventsCount, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    logger.LogWarning("Event forwarding service cancelled.");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while forwarding events.");
                }
            }
        }

        logger.LogInformation("Event forwarding service stopped.");
    }

    private static Task DelayForwarding(int eventsCount, CancellationToken cancellationToken)
        => Task.Delay(eventsCount == 0 ? TimeSpan.FromMilliseconds(500) : TimeSpan.Zero, cancellationToken);
}