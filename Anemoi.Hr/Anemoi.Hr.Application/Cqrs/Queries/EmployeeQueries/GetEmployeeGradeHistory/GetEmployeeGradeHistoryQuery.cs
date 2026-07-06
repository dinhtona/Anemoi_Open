using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Queries.EmployeeQueries.GetEmployeeGradeHistory;

public sealed record GetEmployeeGradeHistoryQuery(
    EmployeeId EmployeeId) : IQuery<IReadOnlyCollection<EmployeeGradeHistoryResponse>>;
