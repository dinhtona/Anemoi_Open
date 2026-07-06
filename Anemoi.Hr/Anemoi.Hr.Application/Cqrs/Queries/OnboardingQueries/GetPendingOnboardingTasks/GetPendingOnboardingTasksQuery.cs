using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Queries;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Queries.OnboardingQueries.GetPendingOnboardingTasks;

public sealed record GetPendingOnboardingTasksQuery(string? AssignedUserId) : GetManyQuery, IQueryPaged<OnboardingTaskResponse>;
