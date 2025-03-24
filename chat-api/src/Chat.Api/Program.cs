using Chat.Api;
using Chat.Messaging;
using Chat.Messaging.Abstractions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NodaTime;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("ChatUi", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .WithExposedHeaders("Location")
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddDbContext<ChatDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));
builder.Services.TryAddScoped<DbContext>(sp => sp.GetRequiredService<ChatDbContext>());

builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddValidatorsFromAssemblyContaining<MessageDtoValidator>();

builder.Services.AddMessaging<Program>(builder.Configuration);

builder.Services.AddScoped<IEventHandler<MessageSent>, MessageSentHandler>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseRouting();

app.UseCors("ChatUi");

app.MapPost("/api/v1/messages", async (ChatDbContext db, MessageDto dto, IValidator<MessageDto> validator, IEventPublisher publisher, CancellationToken cancellationToken) =>
{
    var validationResult = await validator.ValidateAsync(dto);
    if (!validationResult.IsValid)
    {
        return Results.BadRequest(validationResult.Errors);
    }

    var message = new Message(id: Guid.NewGuid(),
        text: dto.Text,
        sender: new User(dto.Sender.Name, dto.Sender.Email),
        timestamp: SystemClock.Instance.GetCurrentInstant().ToUnixTimeTicks());
    
    // await db.Set<Message>().AddAsync(message, cancellationToken);
    
    await publisher.PublishAsync(new MessageSent(id: message.Id,
        data: new MessageSent.MessageDto(Text: message.Text,
            Sender: new MessageSent.UserDto(message.Sender.Name, message.Sender.Email),
            Timestamp: message.Timestamp)), cancellationToken);
    
    await db.SaveChangesAsync(cancellationToken);
    
    return Results.Created($"/api/v1/messages/{message.Id}", null);
});

app.MapGet("/api/v1/messages", async (ChatDbContext db) =>
{
    var items = await db.Set<Message>().OrderByDescending(x => x.Timestamp).Take(20)
        .OrderBy(x => x.Timestamp)
        .ToListAsync();
    return Results.Ok(items);
});

app.MapHub<ChatHub>("/chat-hub");

app.Run();

namespace Chat.Api
{
    public partial class Program;
}