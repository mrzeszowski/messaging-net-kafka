using Chat.Messaging.Filters;
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
    
        services.AddSingleton(_ => new TypeRepository(typeof(TAssembly)));
        services.AddSingleton<MessageProcessor>();

        services.AddKeyedSingleton<IDistributedLockProvider>(nameof(EventForwarderBackgroundService), new PostgresDistributedSynchronizationProvider(connectionString!));
        services.AddHostedService<EventForwarderBackgroundService>();
        
        services.Configure<KafkaOptions>(configuration.GetSection("Kafka"));

        services.AddScoped(typeof(ObservabilityFilter<>));
        services.AddScoped(typeof(TransactionFilter<>));
        
        return services;
    }
}