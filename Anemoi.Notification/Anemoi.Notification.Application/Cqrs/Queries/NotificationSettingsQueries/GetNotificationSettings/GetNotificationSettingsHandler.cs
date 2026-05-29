using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries.QueryFlow.QueryManyFlow;
using Anemoi.BuildingBlock.Application.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Infrastructure.RequestHandlers.Queries.EntityFramework.EfQueryMany;
using Anemoi.Contract.Notification.Queries.NotificationSettingsQueries.GetNotificationSettings;
using Anemoi.Contract.Notification.Responses;
using Anemoi.Notification.Application.Mappings;
using Anemoi.Notification.Domain.Models;
using OneOf;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Anemoi.Notification.Application.Cqrs.Queries.NotificationSettingsQueries.GetNotificationSettings;

public sealed class GetNotificationSettingsHandler(
    ISqlRepository<NotificationSubscription> sqlRepository,
    NotificationMapper mapper,
    ILogger logger)
    : EfQueryCollectionHandler<NotificationSubscription, GetNotificationSettingsQuery, NotificationSettingResponse>(sqlRepository, logger)
{
    protected override IQueryListFlowBuilder<NotificationSubscription, NotificationSettingResponse> BuildQueryFlow(
        IQueryListFilter<NotificationSubscription, NotificationSettingResponse> fromFlow, GetNotificationSettingsQuery query)
    {
        var targetUserGuid = Guid.Parse(query.UserId);
        return fromFlow
            .WithFilter(x => x.UserId == targetUserGuid)
            .WithSpecialAction(a => a)
            .WithSortFieldWhenNotSet(a => a.Category)
            .WithSortedDirectionWhenNotSet(SortedDirection.Ascending);
    }

    protected override Task<List<NotificationSettingResponse>> MapToResultAsync(
        GetNotificationSettingsQuery query,
        OneOf<List<NotificationSubscription>, List<NotificationSettingResponse>> modelsOrResponses)
    {
        return modelsOrResponses.Match(
            models =>
            {
                var list = models.Select(mapper.ToSettingResponse).ToList();
                var defaultCategories = new[] { "System", "Workspace", "Task" };
                foreach (var cat in defaultCategories)
                {
                    if (!list.Any(x => x.Category.Equals(cat, StringComparison.OrdinalIgnoreCase)))
                    {
                        list.Add(new NotificationSettingResponse
                        {
                            Category = cat,
                            IsEnabled = true
                        });
                    }
                }
                return Task.FromResult(list);
            },
            responses => Task.FromResult(responses)
        );
    }
}

