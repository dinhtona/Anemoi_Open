using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Workflow;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Queries.WorkflowQueries.GetPendingApprovals;

public sealed class GetPendingApprovalsHandler(
    ISqlRepository<WorkflowInstance> instanceRepository,
    ISqlRepository<WorkflowDefinition> definitionRepository,
    IWorkflowRoleResolver roleResolver,
    WorkflowMapper mapper)
    : IQueryHandler<GetPendingApprovalsQuery, PaginationResponse<WorkflowInstanceResponse>>
{
    public async Task<PaginationResponse<WorkflowInstanceResponse>> Handle(
        GetPendingApprovalsQuery request, CancellationToken cancellationToken)
    {
        var pendingInstances = await instanceRepository.GetQueryable()
            .Include(x => x.Steps)
            .Include(x => x.Histories)
            .AsNoTracking()
            .Where(x => x.Status == WorkflowStatusCode.Pending)
            .OrderByDescending(x => x.StartedAt)
            .ToListAsync(cancellationToken);

        var roleValues = pendingInstances
            .Select(i => i.Steps.FirstOrDefault(s => s.Sequence == i.CurrentStep))
            .Where(s => s?.ApproverTypeSnapshot == ApproverType.Role && s.ApproverValueSnapshot is not null)
            .Select(s => s!.ApproverValueSnapshot)
            .Distinct()
            .ToList();

        var userRoleValues = new HashSet<string>();
        foreach (var role in roleValues)
        {
            var result = await roleResolver.ResolveAsync(role, cancellationToken);
            if (result.TryPickT0(out var approvers, out _) &&
                approvers.Any(a => string.Equals(a.UserId.Value.ToString(), request.UserId, StringComparison.OrdinalIgnoreCase)))
            {
                userRoleValues.Add(role);
            }
        }

        var matched = pendingInstances.Where(instance =>
        {
            var step = instance.Steps.FirstOrDefault(s => s.Sequence == instance.CurrentStep);
            if (step is null) return false;

            return step.ApproverTypeSnapshot switch
            {
                ApproverType.SpecificUser => string.Equals(step.ApproverValueSnapshot, request.UserId, StringComparison.OrdinalIgnoreCase),
                ApproverType.DirectManager => string.Equals(step.ApproverUserId, request.UserId, StringComparison.OrdinalIgnoreCase),
                ApproverType.Role => userRoleValues.Contains(step.ApproverValueSnapshot),
                ApproverType.Permission => true,
                _ => false
            };
        }).ToList();

        var total = matched.Count;
        var pageItems = matched
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var defIds = pageItems
            .Where(x => x.WorkflowDefinitionId is not null)
            .Select(x => x.WorkflowDefinitionId!.Value)
            .Distinct()
            .ToList();
        var filteredDefs = await definitionRepository.GetQueryable()
            .Where(d => defIds.Contains(d.Id.Value))
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        var defNames = filteredDefs.ToDictionary(d => d.Id.Value, d => d.Name);

        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 10 : request.PageSize;

        return new PaginationResponse<WorkflowInstanceResponse>(
            mapper.ToResponses(pageItems, defNames).ToList(), total);
    }
}
