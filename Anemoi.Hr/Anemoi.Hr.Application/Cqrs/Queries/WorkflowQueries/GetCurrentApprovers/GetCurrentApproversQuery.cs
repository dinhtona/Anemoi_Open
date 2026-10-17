using System.Collections.Generic;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Queries.WorkflowQueries.GetCurrentApprovers;

public sealed record GetCurrentApproversQuery(string Id)
    : IQueryOne<List<CurrentApproverResponse>>;
