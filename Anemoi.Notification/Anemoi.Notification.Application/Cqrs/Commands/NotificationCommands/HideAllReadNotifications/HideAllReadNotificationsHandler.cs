using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Application.Results;
using Anemoi.Contract.Notification.Commands.NotificationCommands.HideAllReadNotifications;
using Anemoi.Contract.Notification.Errors;
using Anemoi.Notification.Domain.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;
using Serilog;

namespace Anemoi.Notification.Application.Cqrs.Commands.NotificationCommands.HideAllReadNotifications;

public sealed class HideAllReadNotificationsHandler(
    ISqlRepository<NotificationHistory> sqlRepository,
    ILogger logger)
    : IRequestHandler<HideAllReadNotificationsCommand, OneOf<None, ErrorDetailResponse>>
{
    public async Task<OneOf<None, ErrorDetailResponse>> Handle(
        HideAllReadNotificationsCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userGuid = Guid.Parse(request.UserId);
            var now = DateTime.UtcNow;

            await sqlRepository.GetQueryable()
                .Where(x => x.UserId == userGuid && x.IsRead && !x.IsHidden)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(x => x.IsHidden, true)
                        .SetProperty(x => x.HiddenAt, now),
                    cancellationToken);

            return None.Value;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error in HideAllReadNotificationsHandler for User: {UserId}", request.UserId);
            return NotificationErrorDetail.NotificationError.HideFailed().ToErrorDetailResponse();
        }
    }
}
