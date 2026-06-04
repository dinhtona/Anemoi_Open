using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Application.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Leaves;

namespace Anemoi.Hr.Application.Cqrs.Queries.LeaveRequestQueries.GetLeaveRequests;

public sealed class GetLeaveRequestsHandler(ISqlRepository<LeaveRequest> repository, LeaveMapper mapper)
    : IQueryHandler<GetLeaveRequestsQuery, PaginationResponse<LeaveRequestResponse>>
{
    public async Task<PaginationResponse<LeaveRequestResponse>> Handle(GetLeaveRequestsQuery request,
        CancellationToken cancellationToken)
    {
        var page = await repository.GetManyByConditionWithPaginationAsync(
            x => (request.EmployeeId == null || x.EmployeeId == request.EmployeeId) &&
                (string.IsNullOrEmpty(request.StatusCode) || x.StatusCode == request.StatusCode) &&
                (request.FromDate == null || x.StartDate >= request.FromDate) &&
                (request.ToDate == null || x.EndDate <= request.ToDate),
            q => q.OrderByWithDynamic(request.SortedFieldName, x => x.CreatedAt,
                    request.SortedDirection ?? SortedDirection.Descending)
                .Offset(request.GetSkip())
                .Limit(request.GetTake()),
            cancellationToken);

        return new PaginationResponse<LeaveRequestResponse>(
            page.Items.Select(mapper.ToLeaveRequestResponse).ToList(),
            page.TotalRecord);
    }
}
