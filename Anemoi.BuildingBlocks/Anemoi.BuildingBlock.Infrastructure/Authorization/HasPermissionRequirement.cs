using Microsoft.AspNetCore.Authorization;

namespace Anemoi.BuildingBlock.Infrastructure.Authorization;

public sealed record HasPermissionRequirement(string Permission) : IAuthorizationRequirement;
