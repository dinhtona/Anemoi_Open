using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Cqrs.Common.Dtos;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Domain.Probation;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Hr.Application.Cqrs.Queries.ProbationQueries.GetProbations;

public sealed class GetProbationsHandler(
    ISqlRepository<ProbationRecord> repository,
    ProbationRecordMapper mapper)
    : IQueryHandler<GetProbationsQuery, PaginationResponse<ProbationRecordDto>>
{
    public async Task<PaginationResponse<ProbationRecordDto>> Handle(
        GetProbationsQuery request, CancellationToken cancellationToken)
    {
        var query = repository.GetQueryable()
            .Include(x => x.Employee)
            .AsQueryable();

        query = query.Where(x =>
            (request.EmployeeId == null || x.EmployeeId == request.EmployeeId) &&
            (string.IsNullOrEmpty(request.StatusCode) || x.StatusCode == request.StatusCode) &&
            (!request.StartDateFrom.HasValue || x.StartDate >= request.StartDateFrom.Value) &&
            (!request.StartDateTo.HasValue || x.StartDate <= request.StartDateTo.Value));

        var total = await query.LongCountAsync(cancellationToken);

        var paged = query.OrderByDescending(x => x.StartDate);
        var skip = request.GetSkip();
        if (skip.HasValue)
            paged = (IOrderedQueryable<ProbationRecord>)paged.Skip(skip.Value);
        var take = request.GetTake();
        if (take.HasValue)
            paged = (IOrderedQueryable<ProbationRecord>)paged.Take(take.Value);

        var items = await paged.ToListAsync(cancellationToken);

        return new PaginationResponse<ProbationRecordDto>(
            items.Select(mapper.ToDto).ToList(),
            total);
    }
}
