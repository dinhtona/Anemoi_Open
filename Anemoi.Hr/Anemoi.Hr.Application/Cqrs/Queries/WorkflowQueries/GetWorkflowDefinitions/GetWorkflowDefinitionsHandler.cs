using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Workflow;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Hr.Application.Cqrs.Queries.WorkflowQueries.GetWorkflowDefinitions;

public sealed class GetWorkflowDefinitionsHandler(
    ISqlRepository<WorkflowDefinition> repository,
    WorkflowMapper mapper)
    : IQueryHandler<GetWorkflowDefinitionsQuery, PaginationResponse<WorkflowDefinitionResponse>>
{
    public async Task<PaginationResponse<WorkflowDefinitionResponse>> Handle(
        GetWorkflowDefinitionsQuery request, CancellationToken cancellationToken)
    {
        var query = repository.GetQueryable()
            .Include(x => x.Steps.OrderBy(s => s.Sequence))
            .AsQueryable();

        if (request.IsActive.HasValue)
            query = query.Where(x => x.IsActive == request.IsActive.Value);
        if (!string.IsNullOrEmpty(request.WorkflowTypeCode))
            query = query.Where(x => x.WorkflowTypeCode == request.WorkflowTypeCode);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 10 : request.PageSize;

        return new PaginationResponse<WorkflowDefinitionResponse>(
            mapper.ToResponses(items).ToList(), total);
    }
}
