using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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

    private WebApplicationFactory<Api.Program>? _factory;
    
    public HttpClient HttpClient { get; private set; } = null!;
    
    private static readonly RetryStrategyOptions StrategyOptions = new()
    {
        Name = "TopicExpectations",
        MaxRetryAttempts = 200,
        Delay = TimeSpan.FromMilliseconds(200),
    };

    public readonly IAsyncPolicy RetryPolicy = new ResiliencePipelineBuilder()
        .AddRetry(StrategyOptions)
        .Build()
        .AsAsyncPolicy();
    
    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        await _kafka.StartAsync();

        _factory = new WebApplicationFactory<Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:Postgres", _postgres.GetConnectionString());
            builder.UseSetting("Kafka:BootstrapServers", _kafka.GetBootstrapAddress());
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