using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Queries.OnboardingQueries.GetOnboardingInstances;

public sealed record GetOnboardingInstancesQuery(
    EmployeeId? EmployeeId,
    string? Status,
    DateTime? StartDateFrom,
    DateTime? StartDateTo) : GetManyQuery, IQueryPaged<OnboardingInstanceResponse>;
