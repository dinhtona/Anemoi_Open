using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Queries.PositionQueries.GetPositions;

public sealed record GetPositionsQuery(string SearchKey, DepartmentId DepartmentId, bool? IsActive)
    : GetManyQuery, IQueryPaged<PositionResponse>;
