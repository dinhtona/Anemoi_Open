using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Workflow;
using Anemoi.Hr.ModelIds.ModelIds;
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
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 10 : request.PageSize;

        var baseQuery = instanceRepository.GetQueryable()
            .AsNoTracking()
            .Where(x => x.Status == WorkflowStatusCode.Pending);

        var roleValues = await baseQuery
            .SelectMany(i => i.Steps.Where(s => s.Sequence == i.CurrentStep))
            .Where(s => s.ApproverTypeSnapshot == ApproverType.Role && s.ApproverValueSnapshot != null)
            .Select(s => s.ApproverValueSnapshot!)
            .Distinct()
            .ToListAsync(cancellationToken);

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

        var matchingIds = new HashSet<Guid>();

        var specificUserIds = await baseQuery
            .Where(i => i.Steps.Any(s => s.Sequence == i.CurrentStep
                && s.ApproverTypeSnapshot == ApproverType.SpecificUser
                && s.ApproverValueSnapshot == request.UserId))
            .Select(i => i.Id.Value)
            .ToListAsync(cancellationToken);
        matchingIds.UnionWith(specificUserIds);

        var directManagerIds = await baseQuery
            .Where(i => i.Steps.Any(s => s.Sequence == i.CurrentStep
                && s.ApproverTypeSnapshot == ApproverType.DirectManager
                && s.ApproverUserId == request.UserId))
            .Select(i => i.Id.Value)
            .ToListAsync(cancellationToken);
        matchingIds.UnionWith(directManagerIds);

        if (userRoleValues.Count > 0)
        {
            var roleIds = await baseQuery
                .Where(i => i.Steps.Any(s => s.Sequence == i.CurrentStep
                    && s.ApproverTypeSnapshot == ApproverType.Role
                    && s.ApproverValueSnapshot != null
                    && userRoleValues.Contains(s.ApproverValueSnapshot)))
                .Select(i => i.Id.Value)
                .ToListAsync(cancellationToken);
            matchingIds.UnionWith(roleIds);
        }

        var permissionIds = await baseQuery
            .Where(i => i.Steps.Any(s => s.Sequence == i.CurrentStep
                && s.ApproverTypeSnapshot == ApproverType.Permission))
            .Select(i => i.Id.Value)
            .ToListAsync(cancellationToken);
        matchingIds.UnionWith(permissionIds);

        var total = matchingIds.Count;

        var pageInstanceIds = matchingIds
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(id => new WorkflowInstanceId(id))
            .ToList();

        IReadOnlyList<WorkflowInstance> pageItems;
        if (pageInstanceIds.Count > 0)
        {
            pageItems = await instanceRepository.GetQueryable()
                .Include(x => x.Steps)
                .Include(x => x.Histories)
                .AsNoTracking()
                .Where(x => pageInstanceIds.Contains(x.Id))
                .OrderByDescending(x => x.StartedAt)
                .ToListAsync(cancellationToken);
        }
        else
        {
            pageItems = [];
        }

        var defIds = pageItems
            .Where(x => x.WorkflowDefinitionId is not null)
            .Select(x => x.WorkflowDefinitionId!)
            .Distinct()
            .ToList();
        var filteredDefs = await definitionRepository.GetQueryable()
            .Where(d => defIds.Contains(d.Id))
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        var defNames = filteredDefs.ToDictionary(d => d.Id.Value, d => d.Name);

        return new PaginationResponse<WorkflowInstanceResponse>(
            mapper.ToResponses(pageItems, defNames).ToList(), total);
    }
}
