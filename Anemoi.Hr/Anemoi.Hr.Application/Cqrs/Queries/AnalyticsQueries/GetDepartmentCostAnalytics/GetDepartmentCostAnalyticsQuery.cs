using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using System.Collections.Generic;

namespace Anemoi.Hr.Application.Cqrs.Queries.AnalyticsQueries.GetDepartmentCostAnalytics;

public sealed record GetDepartmentCostAnalyticsQuery
    : IQuery<ICollection<DepartmentCostItem>>;
