using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Contract.Identity.ModelIds;
using Anemoi.Contract.Notification.Errors;
using Anemoi.Contract.Notification.Queries.NotificationPreferenceQueries.GetMyNotificationPreference;
using Anemoi.Contract.Notification.Responses;
using Anemoi.Notification.Application.Mappings;
using Anemoi.Notification.Domain.Models;
using MediatR;
using OneOf;
using Serilog;

namespace Anemoi.Notification.Application.Cqrs.Queries.NotificationPreferenceQueries.GetMyNotificationPreference;

public sealed class GetMyNotificationPreferenceHandler(
    ISqlRepository<NotificationPreference> preferenceRepository,
    ISqlRepository<NotificationSubscription> subscriptionRepository,
    NotificationMapper mapper,
    ILogger logger)
    : IRequestHandler<GetMyNotificationPreferenceQuery, OneOf<NotificationPreferenceResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<NotificationPreferenceResponse, ErrorDetailResponse>> Handle(
        GetMyNotificationPreferenceQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = new UserId(Guid.Parse(request.UserId));

            var preference = await preferenceRepository
                .GetFirstByConditionAsync(x => x.UserId == userId, token: cancellationToken);

            var subscriptions = await subscriptionRepository
                .GetManyByConditionAsync(x => x.UserId == userId, token: cancellationToken);

            if (preference != null)
                return mapper.ToPreferenceResponse(preference, subscriptions);

            return mapper.ToDefaultPreferenceResponse(subscriptions);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error in GetMyNotificationPreferenceHandler for User: {UserId}", request.UserId);
            return NotificationErrorDetail.PreferenceError.SaveFailed().ToErrorDetailResponse();
        }
    }
}
