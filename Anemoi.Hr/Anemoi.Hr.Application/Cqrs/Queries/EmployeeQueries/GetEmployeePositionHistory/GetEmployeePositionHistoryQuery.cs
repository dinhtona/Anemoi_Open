using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Queries.EmployeeQueries.GetEmployeePositionHistory;

public sealed record GetEmployeePositionHistoryQuery(
    EmployeeId EmployeeId) : IQuery<IReadOnlyCollection<EmployeePositionHistoryResponse>>;
