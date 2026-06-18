using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Workflow;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Hr.Application.Cqrs.Queries.WorkflowQueries.GetPendingApprovals;

public sealed class GetPendingApprovalsHandler(
    ISqlRepository<WorkflowInstance> instanceRepository,
    ISqlRepository<WorkflowDefinition> definitionRepository,
    WorkflowMapper mapper)
    : IQueryHandler<GetPendingApprovalsQuery, PaginationResponse<WorkflowInstanceResponse>>
{
    public async Task<PaginationResponse<WorkflowInstanceResponse>> Handle(
        GetPendingApprovalsQuery request, CancellationToken cancellationToken)
    {
        var pendingInstances = await instanceRepository.GetQueryable()
            .Include(x => x.Steps)
            .Include(x => x.Histories)
            .Where(x => x.Status == WorkflowStatusCode.Pending)
            .OrderByDescending(x => x.StartedAt)
            .ToListAsync(cancellationToken);

        var matched = pendingInstances.Where(instance =>
        {
            var step = instance.Steps.FirstOrDefault(s => s.Sequence == instance.CurrentStep);
            if (step is null) return false;

            return step.ApproverTypeSnapshot switch
            {
                ApproverType.SpecificUser => step.ApproverValueSnapshot == request.UserId,
                ApproverType.DirectManager => step.ApproverUserId == request.UserId,
                ApproverType.Role => false,
                ApproverType.Permission => false,
                _ => false
            };
        }).ToList();

        var total = matched.Count;
        var pageItems = matched
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var defIds = pageItems.Select(x => x.WorkflowDefinitionId.Value).Distinct().ToList();
        var defNames = await definitionRepository.GetQueryable()
            .Where(d => defIds.Contains(d.Id.Value))
            .ToDictionaryAsync(d => d.Id.Value, d => d.Name, cancellationToken);

        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 10 : request.PageSize;

        return new PaginationResponse<WorkflowInstanceResponse>(
            mapper.ToResponses(pageItems, defNames).ToList(), total);
    }
}
