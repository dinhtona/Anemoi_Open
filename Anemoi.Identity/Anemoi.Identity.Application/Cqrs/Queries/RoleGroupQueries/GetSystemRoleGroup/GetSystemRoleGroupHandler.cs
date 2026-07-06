using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries.QueryFlow.QueryOneFlow;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.RequestHandlers.Queries.EntityFramework.EfQueryOne;
using Anemoi.Contract.Identity.Errors;
using Anemoi.Contract.Identity.Queries.RoleGroupQueries.GetSystemRoleGroup;
using Anemoi.Contract.Identity.Responses;
using Anemoi.Identity.Application.Mappings;
using Anemoi.Identity.Domain.Models;
using System.Linq;
using Serilog;

namespace Anemoi.Identity.Application.Cqrs.Queries.RoleGroupQueries.GetSystemRoleGroup;

public sealed class GetSystemRoleGroupHandler(
    ISqlRepository<RoleGroup> sqlRepository,
    IdentityMapper mapper,
    ILogger logger)
    : EfQueryOneHandler<RoleGroup, GetSystemRoleGroupQuery, RoleGroupResponse>(sqlRepository, logger)
{
    protected override IQueryOneFlowBuilder<RoleGroup, RoleGroupResponse> BuildQueryFlow(
        IQueryOneFilter<RoleGroup, RoleGroupResponse> fromFlow, GetSystemRoleGroupQuery query)
        => fromFlow
            .WithFilter(roleGroup => roleGroup.Id == query.RoleGroupId &&
                !roleGroup.RoleGroupClaims.Any(claim => claim.Key == AuthorizationClaimTypes.WorkspaceId))
            .WithSpecialAction(mapper.ProjectToRoleGroupResponse)
            .WithErrorIfNull(IdentityErrorDetail.RoleGroupError.NotFound());
}
