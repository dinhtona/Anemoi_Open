using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries.QueryFlow.QueryManyFlow;
using Anemoi.BuildingBlock.Application.Queries;
using Anemoi.BuildingBlock.Application.RequestHandlers.Queries.EntityFramework.EfQueryMany;
using Anemoi.Contract.Workspace.ModelIds;
using Anemoi.Contract.Workspace.Queries.OrganizationQueries.GetOrganizations;
using Anemoi.Contract.Workspace.Responses;
using Anemoi.Workspace.Application.Mappings;
using Serilog;
using Anemoi.Workspace.Domain.Models;

namespace Anemoi.Workspace.Application.Cqrs.Queries.OrganizationQueries.GetOrganizations;

public sealed class GetOrganizationsHandler(
    ISqlRepository<Organization> sqlRepository,
    WorkspaceMapper mapper,
    ILogger logger,
    IWorkspaceIdGetter workspaceIdGetter)
    : EfQueryPaginationHandler<Organization, GetOrganizationsQuery, OrganizationResponse>(sqlRepository, logger)
{
    protected override IQueryListFlowBuilder<Organization, OrganizationResponse> BuildQueryFlow(
        IQueryListFilter<Organization, OrganizationResponse> fromFlow, GetOrganizationsQuery query)
        => fromFlow
            .WithFilter(x => x.WorkspaceId == new WorkspaceId(Guid.Parse(workspaceIdGetter.WorkspaceId)))
            .WithSpecialAction(mapper.ProjectToOrganizationResponse)
            .WithSortFieldWhenNotSet(x => x.Id)
            .WithSortedDirectionWhenNotSet(SortedDirection.Ascending);
}