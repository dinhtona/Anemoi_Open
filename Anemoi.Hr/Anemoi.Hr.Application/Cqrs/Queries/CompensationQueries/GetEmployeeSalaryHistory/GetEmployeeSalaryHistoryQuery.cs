using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using System.Collections.Generic;

namespace Anemoi.Hr.Application.Cqrs.Queries.CompensationQueries.GetEmployeeSalaryHistory;

public sealed record GetEmployeeSalaryHistoryQuery(
    EmployeeId EmployeeId) : IQuery<IReadOnlyCollection<EmployeeSalaryResponse>>;
