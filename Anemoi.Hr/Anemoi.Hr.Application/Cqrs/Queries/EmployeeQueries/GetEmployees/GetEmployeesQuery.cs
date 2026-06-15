using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Queries.EmployeeQueries.GetEmployees;

public sealed record GetEmployeesQuery(string? SearchKey, DepartmentId? DepartmentId, PositionId? PositionId)
    : GetManyQuery, IQueryPaged<EmployeeResponse>;
