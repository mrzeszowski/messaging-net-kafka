using Chat.Api;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
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
    options.UseNpgsql(builder.Configuration.GetConnectionString("ChatDb")));

builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddValidatorsFromAssemblyContaining<MessageDtoValidator>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseRouting();

app.UseCors("ChatUi");

app.MapPost("/api/v1/messages", async (ChatDbContext db, MessageDto dto, IValidator<MessageDto> validator, IHubContext<ChatHub> hubContext) =>
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
    
    db.Set<Message>().Add(message);
    await db.SaveChangesAsync();

    return Results.Created($"/api/v1/messages/{message.Id}", null);
});

app.MapGet("/api/v1/messages", async (ChatDbContext db) =>
{
    var items = await db.Set<Message>().OrderBy(x => x.Timestamp).Take(20).ToListAsync();
    return Results.Ok(items);
});

app.MapHub<ChatHub>("/chat-hub");

app.Run();