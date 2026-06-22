using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Cqrs.Common.Dtos;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Domain.Separations;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Hr.Application.Cqrs.Queries.SeparationQueries.GetSeparations;

public sealed class GetSeparationsHandler(
    ISqlRepository<EmployeeSeparation> repository,
    EmployeeSeparationMapper mapper)
    : IQueryHandler<GetSeparationsQuery, PaginationResponse<EmployeeSeparationDto>>
{
    public async Task<PaginationResponse<EmployeeSeparationDto>> Handle(
        GetSeparationsQuery request, CancellationToken cancellationToken)
    {
        var query = repository.GetQueryable()
            .Include(x => x.Employee)
            .AsQueryable();

        query = query.Where(x =>
            (request.EmployeeId == null || x.EmployeeId == request.EmployeeId) &&
            (string.IsNullOrEmpty(request.StatusCode) || x.StatusCode == request.StatusCode) &&
            (string.IsNullOrEmpty(request.SeparationType) || x.SeparationTypeCode == request.SeparationType));

        var total = await query.LongCountAsync(cancellationToken);

        var paged = query.OrderByDescending(x => x.CreatedAt);
        var skip = request.GetSkip();
        if (skip.HasValue)
            paged = (IOrderedQueryable<EmployeeSeparation>)paged.Skip(skip.Value);
        var take = request.GetTake();
        if (take.HasValue)
            paged = (IOrderedQueryable<EmployeeSeparation>)paged.Take(take.Value);

        var items = await paged.ToListAsync(cancellationToken);

        return new PaginationResponse<EmployeeSeparationDto>(
            items.Select(mapper.ToDto).ToList(),
            total);
    }
}
