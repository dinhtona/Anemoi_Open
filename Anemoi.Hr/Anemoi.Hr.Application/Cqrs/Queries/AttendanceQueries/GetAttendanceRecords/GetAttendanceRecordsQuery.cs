using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using System.Collections.Generic;

namespace Anemoi.Hr.Application.Cqrs.Queries.AttendanceQueries.GetAttendanceRecords;

public sealed record GetAttendanceRecordsQuery(
    AttendancePeriodId? AttendancePeriodId = null,
    EmployeeId? EmployeeId = null) : IQueryOne<IReadOnlyCollection<AttendanceRecordResponse>>;
