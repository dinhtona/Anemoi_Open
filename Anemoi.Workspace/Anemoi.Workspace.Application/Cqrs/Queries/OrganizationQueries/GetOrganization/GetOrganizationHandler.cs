using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries.QueryFlow.QueryOneFlow;
using Anemoi.BuildingBlock.Application.RequestHandlers.Queries.EntityFramework.EfQueryOne;
using Anemoi.Contract.Workspace.Errors;
using Anemoi.Contract.Workspace.Queries.OrganizationQueries.GetOrganization;
using Anemoi.Contract.Workspace.ModelIds;
using Anemoi.Contract.Workspace.Responses;
using Anemoi.Workspace.Application.Mappings;
using Serilog;
using Anemoi.Workspace.Domain.Models;

namespace Anemoi.Workspace.Application.Cqrs.Queries.OrganizationQueries.GetOrganization;

public sealed class
    GetOrganizationHandler(ISqlRepository<Organization> sqlRepository, IWorkspaceIdGetter workspaceIdGetter,
        WorkspaceMapper mapper, ILogger logger)
    : EfQueryOneHandler<Organization, GetOrganizationQuery, OrganizationResponse>(sqlRepository, logger)
{
    protected override IQueryOneFlowBuilder<Organization, OrganizationResponse> BuildQueryFlow(
        IQueryOneFilter<Organization, OrganizationResponse> fromFlow, GetOrganizationQuery query)
        => fromFlow
            .WithFilter(x => x.Id == query.Id &&
                             x.WorkspaceId == new WorkspaceId(Guid.Parse(workspaceIdGetter.WorkspaceId)))
            .WithSpecialAction(mapper.ProjectToOrganizationResponse)
            .WithErrorIfNull(WorkspaceErrorDetail.OrganizationError.NotFound());
}
