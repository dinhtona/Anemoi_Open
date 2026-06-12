using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Queries.CalendarManagementQueries.PublicHoliday.GetPublicHolidayById;

public sealed record GetPublicHolidayByIdQuery(
    PublicHolidayId Id) : IQuery<PublicHolidayResponse>;
