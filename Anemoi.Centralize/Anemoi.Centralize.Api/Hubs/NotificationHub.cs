using Anemoi.Centralize.Application.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Resources;
using Anemoi.BuildingBlock.Application.Authorization;
using Anemoi.BuildingBlock.Application.Helpers;

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

    public async Task JoinWorkspace(string workspaceId)
    {
        if (string.IsNullOrEmpty(workspaceId)) return;

        if (!UserHasWorkspaceClaim(workspaceId))
        {
            logger.LogWarning("User {UserId} unauthorized connection attempt to Workspace group: Workspace-{WorkspaceId}", Context.UserIdentifier, workspaceId);
            throw new HubException(localizer["HubJoinForbidden"].Value);
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, $"Workspace-{workspaceId}");
        logger.LogInformation("User {UserId} joined Workspace group: Workspace-{WorkspaceId}", Context.UserIdentifier, workspaceId);
    }

    public async Task LeaveWorkspace(string workspaceId)
    {
        if (string.IsNullOrEmpty(workspaceId)) return;
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Workspace-{workspaceId}");
        logger.LogInformation("User {UserId} left Workspace group: Workspace-{WorkspaceId}", Context.UserIdentifier, workspaceId);
    }

    private bool UserHasWorkspaceClaim(string workspaceId)
    {
        if (Context.User == null) return false;
        if (IsAdministrator()) return true;

        var workspaceClaims = Context.User.Claims
            .Where(x => string.Equals(x.Type, "workspaceId", StringComparison.OrdinalIgnoreCase))
            .Select(x => x.Value);

        return workspaceClaims.Any(w => string.Equals(w, workspaceId, StringComparison.OrdinalIgnoreCase));
    }

    private bool CanObserveUsers() =>
        Context.User?.HasClaim("applicationPolicyInternal", "Internal") == true &&
        (IsAdministrator() || Context.User.IsInRole(Permissions.UserRead));

    private bool IsAdministrator() =>
        Context.User?.IsInRole(SystemRoles.Administrator) == true ||
        Context.User?.Claims.Any(claim =>
            string.Equals(claim.Type, AuthorizationClaimTypes.RoleGroup, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(claim.Value, SystemRoles.Administrator, StringComparison.OrdinalIgnoreCase)) == true;
}
