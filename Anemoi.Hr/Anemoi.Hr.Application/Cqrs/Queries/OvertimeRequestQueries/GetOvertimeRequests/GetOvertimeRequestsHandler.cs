using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Overtime;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Hr.Application.Cqrs.Queries.OvertimeRequestQueries.GetOvertimeRequests;

public sealed class GetOvertimeRequestsHandler(
    ISqlRepository<OvertimeRequest> overtimeRequestRepository,
    OvertimeMapper mapper)
    : IQueryHandler<GetOvertimeRequestsQuery, PaginationResponse<OvertimeRequestResponse>>
{
    public async Task<PaginationResponse<OvertimeRequestResponse>> Handle(GetOvertimeRequestsQuery request,
        CancellationToken cancellationToken)
    {
        var query = overtimeRequestRepository.GetQueryable().AsNoTracking()
            .Where(x => x.EmployeeId == request.EmployeeId);

        if (!string.IsNullOrEmpty(request.Status))
            query = query.Where(x => x.Status == request.Status);

        if (request.OvertimeDate.HasValue)
            query = query.Where(x => x.OvertimeDate == request.OvertimeDate.Value);

        query = (request.SortBy?.ToLower(), request.SortDirection?.ToLower()) switch
        {
            ("overtimedate", "desc") => query.OrderByDescending(x => x.OvertimeDate),
            ("overtimedate", "asc") => query.OrderBy(x => x.OvertimeDate),
            ("starttime", "desc") => query.OrderByDescending(x => x.StartTime),
            ("starttime", "asc") => query.OrderBy(x => x.StartTime),
            ("endtime", "desc") => query.OrderByDescending(x => x.EndTime),
            ("endtime", "asc") => query.OrderBy(x => x.EndTime),
            ("createdat", "desc") => query.OrderByDescending(x => x.CreatedAt),
            ("createdat", "asc") => query.OrderBy(x => x.CreatedAt),
            ("status", "desc") => query.OrderByDescending(x => x.Status),
            ("status", "asc") => query.OrderBy(x => x.Status),
            _ => query.OrderByDescending(x => x.CreatedAt)
        };

        var totalRecords = await query.LongCountAsync(cancellationToken);
        var rows = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PaginationResponse<OvertimeRequestResponse>(
            rows.Select(mapper.ToOvertimeRequestResponse).ToList(),
            totalRecords);
    }
}
