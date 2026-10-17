using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Cqrs.Common.Dtos;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Queries.ProbationQueries.GetProbations;

public sealed record GetProbationsQuery(
    EmployeeId? EmployeeId,
    string? StatusCode,
    DateOnly? StartDateFrom,
    DateOnly? StartDateTo) : GetManyQuery, IQueryOne<PaginationResponse<ProbationRecordDto>>;
