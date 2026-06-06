using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Queries.AttendanceQueries.GetEmployeeAttendanceSummary;

public sealed record GetEmployeeAttendanceSummaryQuery(
    AttendancePeriodId AttendancePeriodId,
    EmployeeId EmployeeId) : IQueryOne<AttendanceSummaryResponse>;
