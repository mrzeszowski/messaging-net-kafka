using Chat.Messaging.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Chat.Messaging;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMessaging<TAssembly>(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IEventPublisher, EventPublisher>();
        services.AddScoped<IProducer, KafkaProducer>();
        
        return services;
    }
}