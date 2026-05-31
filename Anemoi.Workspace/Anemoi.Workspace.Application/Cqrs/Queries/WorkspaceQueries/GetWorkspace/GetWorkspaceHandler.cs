using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries.QueryFlow.QueryOneFlow;
using Anemoi.BuildingBlock.Application.RequestHandlers.Queries.EntityFramework.EfQueryOne;
using Anemoi.Contract.Workspace.Errors;
using Anemoi.Contract.Workspace.Queries.WorkspaceQueries.GetWorkspace;
using Anemoi.Contract.Workspace.Responses;
using Anemoi.Workspace.Application.Mappings;
using Serilog;

namespace Anemoi.Workspace.Application.Cqrs.Queries.WorkspaceQueries.GetWorkspace;

public sealed class GetWorkspaceHandler(
    ISqlRepository<Anemoi.Workspace.Domain.Models.Workspace> sqlRepository,
    WorkspaceMapper mapper,
    ILogger logger)
    : EfQueryOneHandler<Anemoi.Workspace.Domain.Models.Workspace, GetWorkspaceQuery, WorkspaceResponse>(sqlRepository,
        logger)
{
    protected override IQueryOneFlowBuilder<Anemoi.Workspace.Domain.Models.Workspace, WorkspaceResponse> BuildQueryFlow(
        IQueryOneFilter<Anemoi.Workspace.Domain.Models.Workspace, WorkspaceResponse> fromFlow, GetWorkspaceQuery query)
        => fromFlow
            .WithFilter(x => x.Id == query.Id)
            .WithSpecialAction(mapper.ProjectToWorkspaceResponse)
            .WithErrorIfNull(WorkspaceErrorDetail.WorkspaceError.NotFound());
}