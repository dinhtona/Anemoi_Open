using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Cqrs.Common.Dtos;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Domain.Employees;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Hr.Application.Cqrs.Queries.LifecycleQueries.GetEmployeeTimeline;

public sealed class GetEmployeeTimelineHandler(
    ISqlRepository<EmployeeHistory> repository,
    EmployeeHistoryMapper mapper)
    : IQueryHandler<GetEmployeeTimelineQuery, PaginationResponse<EmployeeHistoryDto>>
{
    public async Task<PaginationResponse<EmployeeHistoryDto>> Handle(
        GetEmployeeTimelineQuery request, CancellationToken cancellationToken)
    {
        var query = repository.GetQueryable()
            .AsQueryable();

        query = query.Where(x =>
            (request.EmployeeId == null || x.EmployeeId == request.EmployeeId) &&
            (string.IsNullOrEmpty(request.EventType) || x.EventType == request.EventType) &&
            (string.IsNullOrEmpty(request.EntityType) || x.EntityType == request.EntityType) &&
            (!request.DateFrom.HasValue || x.OccurredAt >= request.DateFrom.Value) &&
            (!request.DateTo.HasValue || x.OccurredAt <= request.DateTo.Value));

        var total = await query.LongCountAsync(cancellationToken);

        var paged = query.OrderByDescending(x => x.OccurredAt);
        var skip = request.GetSkip();
        if (skip.HasValue)
            paged = (IOrderedQueryable<EmployeeHistory>)paged.Skip(skip.Value);
        var take = request.GetTake();
        if (take.HasValue)
            paged = (IOrderedQueryable<EmployeeHistory>)paged.Take(take.Value);

        var items = await paged.ToListAsync(cancellationToken);

        return new PaginationResponse<EmployeeHistoryDto>(
            items.Select(mapper.ToDto).ToList(),
            total);
    }
}
