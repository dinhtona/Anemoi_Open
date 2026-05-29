using Anemoi.Centralize.Application.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Anemoi.Centralize.Api.Hubs;

[Authorize]
public sealed class NotificationHub(ILogger<NotificationHub> logger, IConnectedUsersRegistry registry) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(userId))
        {
            registry.AddUser(userId);
            // Broadcast UserOnline event to the Administrators group
            await Clients.Group("Administrators").SendAsync("UserOnline", userId);
        }
        logger.LogInformation("SignalR Client Connected: User {UserId}, ConnectionId {ConnectionId}", userId, Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var userId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(userId))
        {
            registry.RemoveUser(userId);
            // Broadcast UserOffline event to the Administrators group
            await Clients.Group("Administrators").SendAsync("UserOffline", userId);
        }
        logger.LogInformation("SignalR Client Disconnected: User {UserId}, ConnectionId {ConnectionId}, Error: {Error}", 
            userId, Context.ConnectionId, exception?.Message ?? "None");
        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }

    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }
}
