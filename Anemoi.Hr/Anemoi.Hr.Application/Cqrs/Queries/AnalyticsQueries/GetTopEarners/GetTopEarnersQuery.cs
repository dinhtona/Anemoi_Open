using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using System.Collections.Generic;

namespace Anemoi.Hr.Application.Cqrs.Queries.AnalyticsQueries.GetTopEarners;

public sealed record GetTopEarnersQuery(
    PayrollRunId? PayrollRunId,
    int Top = 10
) : IQuery<ICollection<TopEarnerItem>>;
