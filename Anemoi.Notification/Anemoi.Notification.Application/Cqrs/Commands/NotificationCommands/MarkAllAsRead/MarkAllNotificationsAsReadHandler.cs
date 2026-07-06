using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Application.Results;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Contract.Identity.ModelIds;
using Anemoi.Contract.Notification.Commands.NotificationCommands.MarkAllAsRead;
using Anemoi.Contract.Notification.Errors;
using Anemoi.Notification.Domain.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
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
            var userId = new UserId(Guid.Parse(request.UserId));

            var items = await sqlRepository.GetQueryable()
                .Where(x => x.UserId == userId && !x.IsRead && !x.IsArchived)
                .ToListAsync(cancellationToken);

            foreach (var item in items)
                item.MarkAsRead();

            await unitOfWork.SaveChangesAsync(cancellationToken);
            return None.Value;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error occurred in MarkAllNotificationsAsReadHandler for User: {UserId}", request.UserId);
            return NotificationErrorDetail.NotificationError.UpdateFailed().ToErrorDetailResponse();
        }
    }
}
