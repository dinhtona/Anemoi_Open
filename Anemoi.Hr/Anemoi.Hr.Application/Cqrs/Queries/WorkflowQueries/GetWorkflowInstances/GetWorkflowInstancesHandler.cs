using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Workflow;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Hr.Application.Cqrs.Queries.WorkflowQueries.GetWorkflowInstances;

public sealed class GetWorkflowInstancesHandler(
    ISqlRepository<WorkflowInstance> instanceRepository,
    ISqlRepository<WorkflowDefinition> definitionRepository,
    WorkflowMapper mapper)
    : IQueryHandler<GetWorkflowInstancesQuery, PaginationResponse<WorkflowInstanceResponse>>
{
    public async Task<PaginationResponse<WorkflowInstanceResponse>> Handle(
        GetWorkflowInstancesQuery request, CancellationToken cancellationToken)
    {
        var query = instanceRepository.GetQueryable()
            .Include(x => x.Steps)
            .Include(x => x.Histories)
            .AsQueryable();

        if (!string.IsNullOrEmpty(request.Status))
            query = query.Where(x => x.Status == request.Status);
        if (!string.IsNullOrEmpty(request.EntityType))
            query = query.Where(x => x.EntityType == request.EntityType);
        if (!string.IsNullOrEmpty(request.WorkflowDefinitionId))
        {
            var defId = new WorkflowDefinitionId(Guid.Parse(request.WorkflowDefinitionId));
            query = query.Where(x => x.WorkflowDefinitionId == defId);
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.StartedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        Dictionary<Guid, string> defNames = [];
        if (items.Count != 0)
        {
            var defIds = items
                .Where(x => x.WorkflowDefinitionId is not null)
                .Select(x => x.WorkflowDefinitionId!.Value)
                .Distinct()
                .ToList();
            var allDefs = await definitionRepository.GetQueryable().ToListAsync(cancellationToken);
            defNames = allDefs
                .Where(d => defIds.Contains(d.Id.Value))
                .ToDictionary(d => d.Id.Value, d => d.Name);
        }

        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 10 : request.PageSize;

        return new PaginationResponse<WorkflowInstanceResponse>(
            mapper.ToResponses(items, defNames).ToList(), total);
    }
}
