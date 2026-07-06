using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using OneOf;
using System.Collections.Generic;

namespace Anemoi.Contract.Identity.Queries.UserQueries.ResolveUsersByPermission;

public sealed record PermissionUser(string UserId, string? Email);

public sealed record ResolveUsersByPermissionQuery(string Permission) : IQueryOne<ResolveUsersByPermissionResponse>;

public sealed record ResolveUsersByPermissionResponse(IReadOnlyList<PermissionUser> Users);
