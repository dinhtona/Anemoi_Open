using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries.QueryFlow.QueryManyFlow;
using Anemoi.BuildingBlock.Application.Queries;
using Anemoi.BuildingBlock.Infrastructure.RequestHandlers.Queries.EntityFramework.EfQueryMany;
using Anemoi.Contract.Workspace.Queries.WorkspaceQueries.GetWorkspacesByIds;
using Anemoi.Contract.Workspace.Responses;
using Anemoi.Workspace.Application.Mappings;
using Serilog;

namespace Anemoi.Workspace.Application.Cqrs.Queries.WorkspaceQueries.GetWorkspacesByIds;

public sealed class GetWorkspacesByIdsHandler(
    ISqlRepository<Anemoi.Workspace.Domain.Models.Workspace> sqlRepository,
    WorkspaceMapper mapper,
    ILogger logger)
    : EfQueryCollectionHandler<Anemoi.Workspace.Domain.Models.Workspace, GetWorkspacesByIdsQuery, WorkspaceResponse>(
        sqlRepository, logger)
{
    protected override IQueryListFlowBuilder<Anemoi.Workspace.Domain.Models.Workspace, WorkspaceResponse>
        BuildQueryFlow(
            IQueryListFilter<Anemoi.Workspace.Domain.Models.Workspace, WorkspaceResponse> fromFlow,
            GetWorkspacesByIdsQuery query)
        => fromFlow
            .WithFilter(a => query.Ids.Contains(a.Id))
            .WithSpecialAction(mapper.ProjectToWorkspaceResponse)
            .WithSortFieldWhenNotSet(a => a.Id)
            .WithSortedDirectionWhenNotSet(SortedDirection.Ascending);
}