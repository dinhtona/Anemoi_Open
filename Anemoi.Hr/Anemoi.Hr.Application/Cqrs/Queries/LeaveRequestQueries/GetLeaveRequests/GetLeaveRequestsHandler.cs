using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Leaves;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Hr.Application.Cqrs.Queries.LeaveRequestQueries.GetLeaveRequests;

public sealed class GetLeaveRequestsHandler(ISqlRepository<LeaveRequest> repository, LeaveMapper mapper)
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

        return new PaginationResponse<LeaveRequestResponse>(
            items.Select(mapper.ToLeaveRequestResponse).ToList(),
            total);
    }
}
