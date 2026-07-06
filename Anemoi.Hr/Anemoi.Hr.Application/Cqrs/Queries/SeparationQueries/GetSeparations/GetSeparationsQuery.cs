using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Queries;
using Anemoi.Hr.Application.Cqrs.Common.Dtos;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Queries.SeparationQueries.GetSeparations;

public sealed record GetSeparationsQuery(
    EmployeeId? EmployeeId,
    string? StatusCode,
    string? SeparationType) : GetManyQuery, IQueryPaged<EmployeeSeparationDto>;
