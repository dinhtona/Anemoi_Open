using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Queries.LeavePolicyQueries.GetLeavePolicyById;

public sealed record GetLeavePolicyByIdQuery(LeavePolicyId Id) : IQueryOne<LeavePolicyResponse>;
