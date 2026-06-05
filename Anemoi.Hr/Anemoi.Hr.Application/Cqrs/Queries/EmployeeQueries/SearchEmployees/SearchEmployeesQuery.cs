using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Queries;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Queries.EmployeeQueries.SearchEmployees;

public sealed record SearchEmployeesQuery(string SearchKey) : GetManyQuery, IQueryPaged<EmployeeResponse>;
