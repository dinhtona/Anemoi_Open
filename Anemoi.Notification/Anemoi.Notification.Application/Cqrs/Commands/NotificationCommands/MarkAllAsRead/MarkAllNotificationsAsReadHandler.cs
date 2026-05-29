using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Application.Results;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Contract.Notification.Commands.NotificationCommands.MarkAllAsRead;
using Anemoi.Contract.Notification.Errors;
using Anemoi.Notification.Domain.Models;
using MediatR;
using OneOf;
using Serilog;

namespace Anemoi.Notification.Application.Cqrs.Commands.NotificationCommands.MarkAllAsRead;

public sealed class MarkAllNotificationsAsReadHandler(
    ISqlRepository<NotificationHistory> sqlRepository,
    IUnitOfWork unitOfWork,
    ILogger logger)
    : IRequestHandler<MarkAllNotificationsAsReadCommand, OneOf<None, ErrorDetailResponse>>
{
    public async Task<OneOf<None, ErrorDetailResponse>> Handle(MarkAllNotificationsAsReadCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userGuid = Guid.Parse(request.UserId);
            var unreadNotifications = await sqlRepository
                .GetManyByConditionAsync(x => x.UserId == userGuid && !x.IsRead, token: cancellationToken);

            if (!unreadNotifications.Any())
                return None.Value;

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
                notification.ReadTime = DateTime.UtcNow;
            }

            var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
            return saveResult.Match<OneOf<None, ErrorDetailResponse>>(
                _ => None.Value,
                ex =>
                {
                    logger.Error(ex, "Error occurred while marking all notifications as read for User: {UserId}", request.UserId);
                    return NotificationErrorDetail.NotificationError.UpdateFailed().ToErrorDetailResponse();
                }
            );
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error occurred in MarkAllNotificationsAsReadHandler for User: {UserId}", request.UserId);
            return NotificationErrorDetail.NotificationError.UpdateFailed().ToErrorDetailResponse();
        }
    }
}
