using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Queries.PositionQueries.GetPositionById;

public sealed record GetPositionByIdQuery(
    PositionId Id) : IQueryOne<PositionResponse>;
