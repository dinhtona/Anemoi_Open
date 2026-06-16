using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Queries.OnboardingQueries.GetOnboardingInstanceById;

public sealed record GetOnboardingInstanceByIdQuery(OnboardingInstanceId Id) : IQueryOne<OnboardingInstanceResponse>;
