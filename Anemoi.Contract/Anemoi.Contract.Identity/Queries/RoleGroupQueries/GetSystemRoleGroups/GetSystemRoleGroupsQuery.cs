using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Queries;
using Anemoi.Contract.Identity.Responses;

namespace Anemoi.Contract.Identity.Queries.RoleGroupQueries.GetSystemRoleGroups;

public sealed record GetSystemRoleGroupsQuery(
    string SearchKey,
    List<string> CreatorIds,
    bool? IsDefault)
    : GetManyQuery, IQueryPaged<RoleGroupsResponse>;
