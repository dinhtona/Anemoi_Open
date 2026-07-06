using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Queries.ShiftManagementQueries.GetEmployeeScheduleCalendar;

public sealed record GetEmployeeScheduleCalendarQuery(
    EmployeeId EmployeeId = null,
    DateOnly? FromDate = null,
    DateOnly? ToDate = null) : IQuery<List<EmployeeScheduleCalendarResponse>>;
