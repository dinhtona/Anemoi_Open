using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Queries.AnalyticsQueries.GetAttendanceAnalytics;

public sealed record GetAttendanceAnalyticsQuery : IQuery<AttendanceAnalyticsResponse>;
