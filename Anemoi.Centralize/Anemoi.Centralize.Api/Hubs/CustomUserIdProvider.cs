using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Anemoi.Centralize.Api.Hubs;

public sealed class CustomUserIdProvider : IUserIdProvider
{
    public string GetUserId(HubConnectionContext connection)
    {
        // Use the "id" claim from the JWT token which is where User ID is stored
        return connection.User?.FindFirst("id")?.Value;
    }
}
