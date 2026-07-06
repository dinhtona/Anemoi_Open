using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Queries.LeavePolicyQueries.GetLeavePolicy;

public sealed record GetLeavePolicyQuery(LeavePolicyId Id) : IQueryOne<LeavePolicyResponse>;
