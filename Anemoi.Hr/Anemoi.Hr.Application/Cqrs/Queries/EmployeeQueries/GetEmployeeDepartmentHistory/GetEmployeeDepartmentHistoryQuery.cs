using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Queries.EmployeeQueries.GetEmployeeDepartmentHistory;

public sealed record GetEmployeeDepartmentHistoryQuery(
    EmployeeId EmployeeId) : IQueryOne<IReadOnlyCollection<EmployeeDepartmentHistoryResponse>>;
