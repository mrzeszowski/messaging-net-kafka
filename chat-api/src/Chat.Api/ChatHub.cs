using Microsoft.AspNetCore.SignalR;

namespace Chat.Api;

internal class ChatHub : Hub
{
    public async Task Send(string message)
    {
        await Clients.All.SendAsync("Subscribe", message);
    }
}