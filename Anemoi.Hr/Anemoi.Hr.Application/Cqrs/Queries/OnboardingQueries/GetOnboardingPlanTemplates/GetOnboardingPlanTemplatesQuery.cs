using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Queries;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Queries.OnboardingQueries.GetOnboardingPlanTemplates;

public sealed record GetOnboardingPlanTemplatesQuery(string? Status) : GetManyQuery, IQueryPaged<OnboardingPlanTemplateResponse>;
