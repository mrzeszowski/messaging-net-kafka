using System.Text;
using System.Text.Json;
using Chat.Api;
using Chat.Messaging;
using Chat.Messaging.Abstractions;
using Chat.Messaging.Kafka;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Retry;
using Testcontainers.Kafka;
using Testcontainers.PostgreSql;

namespace Chat.Integration.Tests;

public class ApiFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:latest")
        .Build();

    private readonly KafkaContainer _kafka = new KafkaBuilder()
        .WithImage("confluentinc/cp-kafka:latest")
        .Build();

    private WebApplicationFactory<Program>? _factory;
    
    public HttpClient HttpClient { get; private set; } = null!;
    public EventsCollector EventsCollector { get; private set; }
    
    private static readonly RetryStrategyOptions StrategyOptions = new()
    {
        Name = "Api Fixture Retry Strategy",
        MaxRetryAttempts = 200,
        Delay = TimeSpan.FromMilliseconds(200),
    };

    public readonly IAsyncPolicy RetryPolicy = new ResiliencePipelineBuilder()
        .AddRetry(StrategyOptions)
        .Build()
        .AsAsyncPolicy();

    public async Task PublishEvent<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IEvent
    {
        await using var scope = _factory!.Services.CreateAsyncScope();
        var producer = scope.ServiceProvider.GetRequiredService<KafkaProducer>();
        
        var message = new ProducerMessage(
            id: Guid.NewGuid().ToString(),
            type: typeof(TEvent).FullName!,
            payload: Encoding.UTF8.GetBytes(JsonSerializer.Serialize(@event)),
            topic: "chat.events", // todo this needs to be managed
            partitionKey: Encoding.UTF8.GetBytes(@event.Id.ToString()));
        
        await producer.ProduceAsync(message, cancellationToken);
    }

    public async Task ShouldBePublished<TEvent>(Func<TEvent, bool> predicate)
        where TEvent : IEvent
    {
        await RetryPolicy.ExecuteAsync(() =>
        {
            Assert.True(EventsCollector.Contains(predicate));
            return Task.CompletedTask;
        });
    }
    
    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        await _kafka.StartAsync();
        
        EventsCollector = new EventsCollector();
        
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:Postgres", _postgres.GetConnectionString());
            builder.UseSetting("Kafka:BootstrapServers", _kafka.GetBootstrapAddress());

            builder.ConfigureServices(services =>
            {
                services.AddScoped<KafkaProducer>();
                services.Decorate<IProducer, TestProducerDecorator>();
                services.AddSingleton(_ => EventsCollector);
            });
        });
        
        await using (var scope  = _factory.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<DbContext>();
            await dbContext.Database.MigrateAsync();
        }
        
        HttpClient = _factory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        HttpClient.Dispose();
        await _factory!.DisposeAsync();
        
        await _postgres.DisposeAsync();
        await _kafka.DisposeAsync();
    }
}

public class EventsCollector
{
    private readonly Dictionary<string, List<IEvent>> _events = new();
    
    public void Add(IEvent @event)
    {
        var type = @event.GetType().FullName!;

        if (!_events.TryGetValue(type, out var events))
            _events.Add(type, [@event]);
        else
            events.Add(@event);

    }

    public bool Contains<T>(Func<T, bool> predicate)
    {
        var type = typeof(T).FullName!;
        _events.TryGetValue(type, out var events);
        return events is not null && events.Select(x => (T)x).Any(predicate);
    }
}

internal class TestProducerDecorator(IProducer producer, TypeRepository typeRepository, EventsCollector eventsCollector) : IProducer
{
    public async Task ProduceAsync(ProducerMessage producerMessage, CancellationToken cancellationToken)
    {
        await producer.ProduceAsync(producerMessage, cancellationToken);
        
        var type = typeRepository.Types.Single(x => x.FullName == producerMessage.Type);
        var @event = (IEvent)JsonSerializer.Deserialize(Encoding.UTF8.GetString(producerMessage.Payload), type)!;
        eventsCollector.Add(@event);
    }
}