using Chat.Messaging.HostedServices;
using Chat.Messaging.Kafka;
using Medallion.Threading;
using Medallion.Threading.Postgres;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Chat.Messaging;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMessaging<TAssembly>(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres");
        
        services.AddScoped<IEventPublisher, EventPublisher>();
        services.AddScoped<IEventForwarder, EventForwarder>();
        
        services.AddScoped<IProducer, KafkaProducer>();

        services.AddKeyedSingleton<IDistributedLockProvider>(nameof(EventForwarderBackgroundService), new PostgresDistributedSynchronizationProvider(connectionString!));
        services.AddHostedService<EventForwarderBackgroundService>();
        
        return services;
    }
}