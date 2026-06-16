using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Onboarding;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Queries.OnboardingQueries.GetOnboardingDashboard;

public sealed class GetOnboardingDashboardHandler(
    ISqlRepository<OnboardingInstance> instanceRepository,
    OnboardingMapper mapper)
    : IQueryHandler<GetOnboardingDashboardQuery, OnboardingDashboardResponse>
{
    public async Task<OnboardingDashboardResponse> Handle(
        GetOnboardingDashboardQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var activeInstances = await instanceRepository.GetQueryable()
            .Where(x => x.Status == OnboardingInstanceStatusCode.InProgress)
            .ToListAsync(cancellationToken);

        var totalActive = activeInstances.Count;

        var totalPendingTasks = 0;
        var totalOverdueTasks = 0;
        var totalUpcomingTasks = 0;
        var totalCompletionRateSum = 0.0;

        foreach (var instance in activeInstances)
        {
            var pendingTasks = instance.Tasks.Count(t => t.IsPending());
            totalPendingTasks += pendingTasks;
            totalOverdueTasks += instance.Tasks.Count(t => t.IsPending() && t.DueDate < now);
            totalUpcomingTasks += instance.Tasks.Count(t => t.IsPending() && t.DueDate >= now);
            totalCompletionRateSum += instance.GetCompletionPercentage();
        }

        var completedThisMonth = await instanceRepository.GetQueryable()
            .LongCountAsync(x => x.Status == OnboardingInstanceStatusCode.Completed &&
                                  x.CompletedAt >= monthStart, cancellationToken);

        var avgCompletionRate = totalActive > 0
            ? totalCompletionRateSum / totalActive
            : 0.0;

        return mapper.ToDashboardResponse(
            totalActive,
            totalOverdueTasks,
            totalPendingTasks,
            totalUpcomingTasks,
            (int)completedThisMonth,
            avgCompletionRate);
    }
}
