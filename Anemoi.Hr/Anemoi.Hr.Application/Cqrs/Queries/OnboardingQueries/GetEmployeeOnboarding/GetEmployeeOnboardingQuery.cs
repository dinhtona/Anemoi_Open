using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Queries.OnboardingQueries.GetEmployeeOnboarding;

public sealed record GetEmployeeOnboardingQuery(EmployeeId EmployeeId) : IQueryOne<OnboardingInstanceResponse>;
