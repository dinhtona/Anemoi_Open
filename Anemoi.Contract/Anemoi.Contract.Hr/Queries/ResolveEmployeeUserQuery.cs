#nullable enable

using Anemoi.BuildingBlock.Application.Cqrs.Queries;

namespace Anemoi.Contract.Hr.Queries;

public sealed record ResolveEmployeeUserQuery(string EmployeeId) : IQueryOne<ResolveEmployeeUserResponse>;

public sealed record ResolveEmployeeUserResponse(string? UserId, string? Email);
