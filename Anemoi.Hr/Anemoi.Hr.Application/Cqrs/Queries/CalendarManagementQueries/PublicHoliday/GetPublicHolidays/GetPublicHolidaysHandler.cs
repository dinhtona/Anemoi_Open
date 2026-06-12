using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.CalendarManagement;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Hr.Application.Cqrs.Queries.CalendarManagementQueries.PublicHoliday.GetPublicHolidays;

public sealed class GetPublicHolidaysHandler(
    ISqlRepository<Domain.CalendarManagement.PublicHoliday> publicHolidayRepository,
    CalendarManagementMapper mapper)
    : IQueryHandler<GetPublicHolidaysQuery, PaginationResponse<PublicHolidayResponse>>
{
    public async Task<PaginationResponse<PublicHolidayResponse>> Handle(
        GetPublicHolidaysQuery request,
        CancellationToken cancellationToken)
    {
        var query = publicHolidayRepository.GetQueryable().AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.CountryCode))
            query = query.Where(x => x.CountryCode == request.CountryCode);

        if (request.Year.HasValue)
            query = query.Where(x => x.HolidayDate.Year == request.Year.Value);

        if (request.Month.HasValue)
            query = query.Where(x => x.HolidayDate.Month == request.Month.Value);

        query = (request.SortBy?.ToLower(), request.SortDirection?.ToLower()) switch
        {
            ("name", "desc") => query.OrderByDescending(x => x.Name),
            ("name", "asc") => query.OrderBy(x => x.Name),
            ("holidaydate", "desc") => query.OrderByDescending(x => x.HolidayDate),
            ("holidaydate", "asc") => query.OrderBy(x => x.HolidayDate),
            ("countrycode", "desc") => query.OrderByDescending(x => x.CountryCode),
            ("countrycode", "asc") => query.OrderBy(x => x.CountryCode),
            ("createdat", "desc") => query.OrderByDescending(x => x.CreatedAt),
            ("createdat", "asc") => query.OrderBy(x => x.CreatedAt),
            _ => query.OrderBy(x => x.HolidayDate)
        };

        var totalRecords = await query.LongCountAsync(cancellationToken);
        var rows = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PaginationResponse<PublicHolidayResponse>(
            rows.Select(mapper.ToPublicHolidayResponse).ToList(),
            totalRecords);
    }
}
