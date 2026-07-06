using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Queries;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Queries.CompensationQueries.GetAllowanceTypes;

public sealed record GetAllowanceTypesQuery(string? SearchKey, bool? IsActive)
    : GetManyQuery, IQueryPaged<AllowanceTypeResponse>;
