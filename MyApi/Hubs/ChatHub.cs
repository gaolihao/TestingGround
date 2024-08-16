using Microsoft.AspNetCore.SignalR;

namespace MyApi.Hubs;

public class ChatHub : Hub{
    public override async Task OnConnectedAsync()
    {
        await Clients.All.SendAsync("ReceiveMessage", $"{Context.ConnectionId} has joined");
    }
}
