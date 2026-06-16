using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Queries;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Queries.OnboardingQueries.GetMyOnboardingTasks;

public sealed record GetMyOnboardingTasksQuery(string UserId) : GetManyQuery, IQueryPaged<OnboardingTaskResponse>;
