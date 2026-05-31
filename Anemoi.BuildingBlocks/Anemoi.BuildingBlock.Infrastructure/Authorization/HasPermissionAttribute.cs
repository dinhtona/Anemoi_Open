using Microsoft.AspNetCore.Authorization;

namespace Anemoi.BuildingBlock.Infrastructure.Authorization;

public sealed class HasPermissionAttribute(string permission)
    : AuthorizeAttribute($"{PolicyPrefix}{permission}")
{
    public const string PolicyPrefix = "Permission:";
}
