using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.ShiftManagement;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Hr.Application.Cqrs.Queries.ShiftManagementQueries.GetShiftTemplates;

public sealed class GetShiftTemplatesHandler(
    ISqlRepository<ShiftTemplate> shiftTemplateRepository,
    ShiftManagementMapper mapper)
    : IQueryHandler<GetShiftTemplatesQuery, PaginationResponse<ShiftTemplateResponse>>
{
    public async Task<PaginationResponse<ShiftTemplateResponse>> Handle(
        GetShiftTemplatesQuery request,
        CancellationToken cancellationToken)
    {
        var query = shiftTemplateRepository.GetQueryable().AsNoTracking();

        if (request.IsActive.HasValue)
            query = query.Where(x => x.IsActive == request.IsActive.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(x => x.Name.Contains(request.Search) || x.Code.Contains(request.Search));

        query = (request.SortBy?.ToLower(), request.SortDirection?.ToLower()) switch
        {
            ("code", "desc") => query.OrderByDescending(x => x.Code),
            ("code", "asc") => query.OrderBy(x => x.Code),
            ("name", "desc") => query.OrderByDescending(x => x.Name),
            ("name", "asc") => query.OrderBy(x => x.Name),
            ("starttime", "desc") => query.OrderByDescending(x => x.StartTime),
            ("starttime", "asc") => query.OrderBy(x => x.StartTime),
            ("createdat", "desc") => query.OrderByDescending(x => x.CreatedAt),
            ("createdat", "asc") => query.OrderBy(x => x.CreatedAt),
            _ => query.OrderBy(x => x.Code)
        };

        var totalRecords = await query.LongCountAsync(cancellationToken);
        var rows = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PaginationResponse<ShiftTemplateResponse>(
            rows.Select(mapper.ToShiftTemplateResponse).ToList(),
            totalRecords);
    }
}
