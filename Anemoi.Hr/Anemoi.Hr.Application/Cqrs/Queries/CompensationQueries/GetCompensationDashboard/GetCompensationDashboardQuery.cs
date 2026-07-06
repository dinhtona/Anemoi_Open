using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Queries.CompensationQueries.GetCompensationDashboard;

public sealed record GetCompensationDashboardQuery : IQuery<CompensationDashboardResponse>;
