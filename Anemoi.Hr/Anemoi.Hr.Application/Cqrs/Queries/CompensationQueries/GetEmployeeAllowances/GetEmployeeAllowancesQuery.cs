using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Queries.CompensationQueries.GetEmployeeAllowances;

public sealed record GetEmployeeAllowancesQuery(
    EmployeeId EmployeeId) : IQuery<EmployeeAllowancesResponse>;
