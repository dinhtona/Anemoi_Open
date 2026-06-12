using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Queries.CalendarManagementQueries.CompanyHoliday.GetCompanyHolidays;

public sealed record GetCompanyHolidaysQuery(
    int? Year = null,
    int? Month = null,
    int Page = 1,
    int PageSize = 50,
    string SortBy = null,
    string SortDirection = "asc") : IQuery<PaginationResponse<CompanyHolidayResponse>>;
