using System;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using System.Collections.Generic;

namespace Anemoi.Hr.Application.Cqrs.Queries.AnalyticsQueries.GetHeadcountTrend;

public sealed record GetHeadcountTrendQuery(DateOnly FromDate, DateOnly ToDate)
    : IQuery<ICollection<HeadcountTrendItem>>;
