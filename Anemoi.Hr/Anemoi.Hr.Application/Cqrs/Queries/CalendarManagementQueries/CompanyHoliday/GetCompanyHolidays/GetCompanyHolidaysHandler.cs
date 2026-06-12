using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.CalendarManagement;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Hr.Application.Cqrs.Queries.CalendarManagementQueries.CompanyHoliday.GetCompanyHolidays;

public sealed class GetCompanyHolidaysHandler(
    ISqlRepository<Domain.CalendarManagement.CompanyHoliday> companyHolidayRepository,
    CalendarManagementMapper mapper)
    : IQueryHandler<GetCompanyHolidaysQuery, PaginationResponse<CompanyHolidayResponse>>
{
    public async Task<PaginationResponse<CompanyHolidayResponse>> Handle(
        GetCompanyHolidaysQuery request,
        CancellationToken cancellationToken)
    {
        var query = companyHolidayRepository.GetQueryable().AsNoTracking();

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
            ("createdat", "desc") => query.OrderByDescending(x => x.CreatedAt),
            ("createdat", "asc") => query.OrderBy(x => x.CreatedAt),
            _ => query.OrderBy(x => x.HolidayDate)
        };

        var totalRecords = await query.LongCountAsync(cancellationToken);
        var rows = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PaginationResponse<CompanyHolidayResponse>(
            rows.Select(mapper.ToCompanyHolidayResponse).ToList(),
            totalRecords);
    }
}
