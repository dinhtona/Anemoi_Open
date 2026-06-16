using System;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Application.Results;
using Anemoi.Contract.Notification.Commands.NotificationCommands.HideNotification;
using Anemoi.Contract.Notification.Errors;
using Anemoi.Notification.Domain.Models;
using MediatR;
using OneOf;
using Serilog;

namespace Anemoi.Notification.Application.Cqrs.Commands.NotificationCommands.HideNotification;

public sealed class HideNotificationHandler(
    ISqlRepository<NotificationHistory> sqlRepository,
    IUnitOfWork unitOfWork,
    ILogger logger)
    : IRequestHandler<HideNotificationCommand, OneOf<None, ErrorDetailResponse>>
{
    public async Task<OneOf<None, ErrorDetailResponse>> Handle(
        HideNotificationCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var notification = await sqlRepository
                .GetFirstByConditionAsync(
                    x => x.Id == request.Id && x.UserId == Guid.Parse(request.UserId),
                    token: cancellationToken);

            if (notification == null)
                return NotificationErrorDetail.NotificationError.NotFound().ToErrorDetailResponse();

            notification.Hide();

            var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
            return saveResult.Match<OneOf<None, ErrorDetailResponse>>(
                _ => None.Value,
                ex =>
                {
                    logger.Error(ex, "Failed to hide notification {NotificationId}", request.Id);
                    return NotificationErrorDetail.NotificationError.HideFailed().ToErrorDetailResponse();
                }
            );
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error in HideNotificationHandler for {NotificationId}", request.Id);
            return NotificationErrorDetail.NotificationError.HideFailed().ToErrorDetailResponse();
        }
    }
}
