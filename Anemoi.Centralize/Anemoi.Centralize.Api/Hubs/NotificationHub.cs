using Anemoi.Centralize.Application.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Resources;

namespace Anemoi.Centralize.Api.Hubs;

[Authorize]
public sealed class NotificationHub(
    ILogger<NotificationHub> logger,
    IConnectedUsersRegistry registry,
    IStringLocalizer<SharedResource> localizer) : Hub
{
    private const string AdministratorsGroup = "Administrators";

    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(userId))
        {
            registry.AddUser(userId);
            // Broadcast UserOnline event to the Administrators group
            await Clients.Group(AdministratorsGroup).SendAsync("UserOnline", userId);
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
            await Clients.Group(AdministratorsGroup).SendAsync("UserOffline", userId);
        }
        logger.LogInformation("SignalR Client Disconnected: User {UserId}, ConnectionId {ConnectionId}, Error: {Error}", 
            userId, Context.ConnectionId, exception?.Message ?? "None");
        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinGroup(string groupName)
    {
        if (groupName != AdministratorsGroup || !CanObserveUsers())
            throw new HubException(localizer["HubJoinForbidden"].Value);

        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }

    public async Task LeaveGroup(string groupName)
    {
        if (groupName != AdministratorsGroup)
            throw new HubException(localizer["HubUnknownGroup"].Value);

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }

    private bool CanObserveUsers() =>
        Context.User?.HasClaim("applicationPolicyInternal", "Internal") == true &&
        (Context.User.IsInRole("Administrator") || Context.User.IsInRole("UserQuery"));
}
