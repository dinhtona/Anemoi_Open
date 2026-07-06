using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Queries.OnboardingQueries.GetOnboardingDashboard;

public sealed record GetOnboardingDashboardQuery : IQuery<OnboardingDashboardResponse>;
