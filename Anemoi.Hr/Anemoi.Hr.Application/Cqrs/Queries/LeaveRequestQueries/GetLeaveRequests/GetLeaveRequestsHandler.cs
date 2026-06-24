using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Leaves;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Hr.Application.Cqrs.Queries.LeaveRequestQueries.GetLeaveRequests;

public sealed class GetLeaveRequestsHandler(
    ISqlRepository<LeaveRequest> repository,
    LeaveMapper mapper,
    IWorkflowQueryService workflowQueryService)
    : IQueryHandler<GetLeaveRequestsQuery, PaginationResponse<LeaveRequestResponse>>
{
    public async Task<PaginationResponse<LeaveRequestResponse>> Handle(GetLeaveRequestsQuery request,
        CancellationToken cancellationToken)
    {
        var query = repository.GetQueryable()
            .Include(x => x.ApproverEmployee)
            .AsQueryable();

        query = query.Where(x =>
            (request.EmployeeId == null || x.EmployeeId == request.EmployeeId) &&
            (string.IsNullOrEmpty(request.StatusCode) || x.StatusCode == request.StatusCode) &&
            (request.FromDate == null || x.StartDate >= request.FromDate) &&
            (request.ToDate == null || x.EndDate <= request.ToDate));

        var total = await query.LongCountAsync(cancellationToken);

        var paged = query.OrderByDescending(x => x.CreatedAt);
        var skip = request.GetSkip();
        if (skip.HasValue)
            paged = (IOrderedQueryable<LeaveRequest>)paged.Skip(skip.Value);
        var take = request.GetTake();
        if (take.HasValue)
            paged = (IOrderedQueryable<LeaveRequest>)paged.Take(take.Value);

        var items = await paged.ToListAsync(cancellationToken);

        var responses = items.Select(mapper.ToLeaveRequestResponse).ToList();

        var entityIds = items.Select(x => x.Id.Value).ToList();
        var summaries = await workflowQueryService.GetWorkflowSummariesAsync(
            WorkflowConstants.TargetEntityTypes.LeaveRequest, entityIds, cancellationToken);

        foreach (var (item, response) in items.Zip(responses))
        {
            if (summaries.TryGetValue(item.Id.Value, out var summary))
            {
                response.CurrentApproverName = summary.CurrentApproverName;
                response.CurrentStepName = summary.CurrentStepName;
                response.WorkflowStatus = summary.WorkflowStatus;
            }
        }

        return new PaginationResponse<LeaveRequestResponse>(responses, total);
    }
}
