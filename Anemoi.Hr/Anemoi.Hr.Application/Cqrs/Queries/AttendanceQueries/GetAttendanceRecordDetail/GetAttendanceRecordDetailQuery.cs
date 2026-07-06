using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Queries.AttendanceQueries.GetAttendanceRecordDetail;

public sealed record GetAttendanceRecordDetailQuery(AttendanceRecordId Id) : IQueryOne<AttendanceRecordDetailResponse>;
