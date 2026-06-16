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
using Microsoft.EntityFrameworkCore;
using OneOf;
using Serilog;

namespace Anemoi.Notification.Application.Cqrs.Commands.NotificationCommands.MarkAllAsRead;

public sealed class MarkAllNotificationsAsReadHandler(
    ISqlRepository<NotificationHistory> sqlRepository,
    ILogger logger)
    : IRequestHandler<MarkAllNotificationsAsReadCommand, OneOf<None, ErrorDetailResponse>>
{
    public async Task<OneOf<None, ErrorDetailResponse>> Handle(MarkAllNotificationsAsReadCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userGuid = Guid.Parse(request.UserId);
            var now = DateTime.UtcNow;

            await sqlRepository.GetQueryable()
                .Where(x => x.UserId == userGuid && !x.IsRead && !x.IsArchived)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(x => x.IsRead, true)
                        .SetProperty(x => x.ReadTime, now),
                    cancellationToken);

            return None.Value;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error occurred in MarkAllNotificationsAsReadHandler for User: {UserId}", request.UserId);
            return NotificationErrorDetail.NotificationError.UpdateFailed().ToErrorDetailResponse();
        }
    }
}
