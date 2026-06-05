using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Queries.EmployeeQueries.GetEmployee;

public sealed record GetEmployeeQuery(EmployeeId Id) : IQueryOne<EmployeeResponse>;
