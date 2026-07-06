using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Onboarding;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Queries.OnboardingQueries.GetMyOnboardingTasks;

public sealed class GetMyOnboardingTasksHandler(
    ISqlRepository<OnboardingInstance> instanceRepository,
    OnboardingMapper mapper)
    : IQueryHandler<GetMyOnboardingTasksQuery, PaginationResponse<OnboardingTaskResponse>>
{
    public async Task<PaginationResponse<OnboardingTaskResponse>> Handle(
        GetMyOnboardingTasksQuery request, CancellationToken cancellationToken)
    {
        var query = instanceRepository.GetQueryable()
            .SelectMany(i => i.Tasks, (instance, task) => new { Instance = instance, Task = task })
            .Where(x => x.Task.AssignedUserId == request.UserId);

        var totalRecord = await query.LongCountAsync(cancellationToken);

        var items = await query
            .OrderBy(x => x.Task.DueDate)
            .Offset(request.GetSkip())
            .Limit(request.GetTake())
            .ToListAsync(cancellationToken);

        var responses = items.Select(x =>
        {
            var response = mapper.ToResponse(x.Task);
            response.InstanceId = x.Instance.Id.Value.ToString();
            return response;
        }).ToList();

        return new PaginationResponse<OnboardingTaskResponse>(responses, totalRecord);
    }
}
