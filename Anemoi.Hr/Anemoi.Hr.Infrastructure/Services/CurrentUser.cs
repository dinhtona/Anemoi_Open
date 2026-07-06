#nullable enable

using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.Hr.Application.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Anemoi.Hr.Infrastructure.Services;

public sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public string UserId => httpContextAccessor.HttpContext?.GetUserId() ?? string.Empty;
}
