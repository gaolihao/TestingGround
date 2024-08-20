using Microsoft.AspNetCore.SignalR;

namespace MyApi.Hubs;

public class ChatHub : Hub{
    
    /*
    public override Task OnConnectedAsync()
    {
        //var name = Context.GetHttpContext().Request.Query["name"];
        //return Clients.All.SendAsync("Send", $"{name} joined the chat");
        //await Clients.All.SendAsync("ReceiveMessage", $"{Context.ConnectionId} has joined");
        return Clients.All.SendAsync("Send", "", "Connect");
    }

    public override Task OnDisconnectedAsync(Exception exception)
    {
        //var name = Context.GetHttpContext().Request.Query["name"];
        //return Clients.All.SendAsync("Send", $"{name} left the chat");
        return Clients.All.SendAsync("Send", "Disconnect");
    }
    */
    

    /*
    public Task Send(string name, string message)
    {
        return Clients.All.SendAsync("Send", $"{name}: {message}");
    }

    public Task SendToOthers(string name, string message)
    {
        return Clients.Others.SendAsync("Send", $"{name}: {message}");
    }

    public Task SendToConnection(string connectionId, string name, string message)
    {
        return Clients.Client(connectionId).SendAsync("Send", $"Private message from {name}: {message}");
    }

    public Task SendToGroup(string groupName, string name, string message)
    {
        return Clients.Group(groupName).SendAsync("Send", $"{name}@{groupName}: {message}");
    }

    public Task SendToOthersInGroup(string groupName, string name, string message)
    {
        return Clients.OthersInGroup(groupName).SendAsync("Send", $"{name}@{groupName}: {message}");
    }

    public async Task JoinGroup(string groupName, string name)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

        await Clients.Group(groupName).SendAsync("Send", $"{name} joined {groupName}");
    }

    public async Task LeaveGroup(string groupName, string name)
    {
        await Clients.Group(groupName).SendAsync("Send", $"{name} left {groupName}");

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }

    public Task Echo(string name, string message)
    {
        return Clients.Caller.SendAsync("Send", $"{name}: {message}");
    }
    */

}
