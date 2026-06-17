using System;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Application.Results;
using Anemoi.Contract.Identity.ModelIds;
using Anemoi.Contract.Notification.Commands.NotificationCommands.ArchiveNotification;
using Anemoi.Contract.Notification.Errors;
using Anemoi.Notification.Domain.Models;
using MediatR;
using OneOf;
using Serilog;

namespace Anemoi.Notification.Application.Cqrs.Commands.NotificationCommands.ArchiveNotification;

public sealed class ArchiveNotificationHandler(
    ISqlRepository<NotificationHistory> sqlRepository,
    IUnitOfWork unitOfWork,
    ILogger logger)
    : IRequestHandler<ArchiveNotificationCommand, OneOf<None, ErrorDetailResponse>>
{
    public async Task<OneOf<None, ErrorDetailResponse>> Handle(
        ArchiveNotificationCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = new UserId(Guid.Parse(request.UserId));
            var notification = await sqlRepository
                .GetFirstByConditionAsync(
                    x => x.Id == request.Id && x.UserId == userId,
                    token: cancellationToken);

            if (notification == null)
                return NotificationErrorDetail.NotificationError.NotFound().ToErrorDetailResponse();

            if (notification.IsArchived)
                return NotificationErrorDetail.ArchiveError.AlreadyArchived().ToErrorDetailResponse();

            notification.Archive(userId.Value);

            var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
            return saveResult.Match<OneOf<None, ErrorDetailResponse>>(
                _ => None.Value,
                ex =>
                {
                    logger.Error(ex, "Failed to archive notification {NotificationId}", request.Id);
                    return NotificationErrorDetail.ArchiveError.ArchiveFailed().ToErrorDetailResponse();
                }
            );
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error in ArchiveNotificationHandler for {NotificationId}", request.Id);
            return NotificationErrorDetail.ArchiveError.ArchiveFailed().ToErrorDetailResponse();
        }
    }
}
