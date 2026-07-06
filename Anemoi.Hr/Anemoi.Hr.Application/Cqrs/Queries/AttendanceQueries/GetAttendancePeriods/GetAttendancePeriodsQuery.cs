using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using System.Collections.Generic;

namespace Anemoi.Hr.Application.Cqrs.Queries.AttendanceQueries.GetAttendancePeriods;

public sealed record GetAttendancePeriodsQuery() : IQueryOne<IReadOnlyCollection<AttendancePeriodResponse>>;
