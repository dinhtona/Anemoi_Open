using System;
using Anemoi.BuildingBlock.Application.Errors;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Contract.Notification.Responses;
using Anemoi.Notification.Domain.Models;
using Riok.Mapperly.Abstractions;

namespace Anemoi.Notification.Application.Mappings;

[Mapper]
public partial class NotificationMapper
{
    public NotificationResponse ToNotificationResponse(NotificationHistory history)
    {
        if (history == null) return null;
        return new NotificationResponse
        {
            Id = history.Id.Value.ToString(),
            UserId = history.UserId.ToString(),
            Title = history.Title,
            Content = history.Content,
            Category = history.Category,
            IsRead = history.IsRead,
            CreatedTime = history.CreatedTime,
            ReadTime = history.ReadTime
        };
    }

    public NotificationSettingResponse ToSettingResponse(NotificationSubscription subscription)
    {
        if (subscription == null) return null;
        return new NotificationSettingResponse
        {
            Category = subscription.Category,
            IsEnabled = subscription.IsEnabled
        };
    }

    public ErrorDetailResponse ToErrorDetailResponse(ErrorDetail errorDetail)
    {
        return new ErrorDetailResponse
        {
            Code = errorDetail.Code,
            Messages = errorDetail.Messages
        };
    }
}
