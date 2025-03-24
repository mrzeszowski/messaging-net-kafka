using Chat.Messaging.Abstractions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Chat.Api;

internal class MessageSentHandler(ChatDbContext dbContext, IHubContext<ChatHub> hubContext, ILogger<MessageSentHandler> logger) : IEventHandler<MessageSent>
{
    public async Task HandleAsync(MessageSent @event, CancellationToken cancellationToken = default)
    {
        var dbSet = dbContext.Set<Message>();
        var message = await dbSet.FirstOrDefaultAsync(x => x.Id == @event.Id, cancellationToken: cancellationToken);
        if (message is not null)
            return;

        message = new Message(id: @event.Id,
            text: @event.Data.Text,
            sender: new User(@event.Data.Sender.Name, @event.Data.Sender.Email),
            timestamp: @event.Data.Timestamp);

        await dbContext.AddAsync(message, cancellationToken);
        // await dbContext.SaveChangesAsync(cancellationToken);    
        
        await hubContext.Clients.All.SendAsync("Subscribe", message, cancellationToken);
        
        logger.LogInformation("Message sent handled: ID: {0}, Text: {1}, Sender:{2}", message.Id, message.Text, message.Sender.Email);
    }
}