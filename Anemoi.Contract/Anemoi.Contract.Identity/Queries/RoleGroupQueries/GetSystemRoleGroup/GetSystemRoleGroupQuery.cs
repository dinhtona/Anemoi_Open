using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Contract.Identity.ModelIds;
using Anemoi.Contract.Identity.Responses;

namespace Anemoi.Contract.Identity.Queries.RoleGroupQueries.GetSystemRoleGroup;

public sealed record GetSystemRoleGroupQuery(RoleGroupId RoleGroupId) : IQueryOne<RoleGroupResponse>;
