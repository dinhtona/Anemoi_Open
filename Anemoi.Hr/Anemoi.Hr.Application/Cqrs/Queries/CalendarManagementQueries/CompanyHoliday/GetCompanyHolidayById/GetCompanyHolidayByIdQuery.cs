using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Queries.CalendarManagementQueries.CompanyHoliday.GetCompanyHolidayById;

public sealed record GetCompanyHolidayByIdQuery(
    CompanyHolidayId Id) : IQuery<CompanyHolidayResponse>;
