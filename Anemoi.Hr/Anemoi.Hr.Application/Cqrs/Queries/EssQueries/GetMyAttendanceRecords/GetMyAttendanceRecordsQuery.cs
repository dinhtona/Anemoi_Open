using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Queries.EssQueries.GetMyAttendanceRecords;

public sealed record GetMyAttendanceRecordsQuery(string UserId, string Email) : IQueryOne<IReadOnlyCollection<EssAttendanceRecordResponse>>;
