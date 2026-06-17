using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Application.Results;
using Anemoi.Contract.Identity.ModelIds;
using Anemoi.Contract.Notification.Commands.NotificationCommands.ArchiveAllReadNotifications;
using Anemoi.Contract.Notification.Errors;
using Anemoi.Notification.Domain.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;
using Serilog;

namespace Anemoi.Notification.Application.Cqrs.Commands.NotificationCommands.ArchiveAllReadNotifications;

public sealed class ArchiveAllReadNotificationsHandler(
    ISqlRepository<NotificationHistory> sqlRepository,
    ILogger logger)
    : IRequestHandler<ArchiveAllReadNotificationsCommand, OneOf<None, ErrorDetailResponse>>
{
    public async Task<OneOf<None, ErrorDetailResponse>> Handle(
        ArchiveAllReadNotificationsCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = new UserId(Guid.Parse(request.UserId));
            var now = DateTime.UtcNow;

            await sqlRepository.GetQueryable()
                .Where(x => x.UserId == userId && x.IsRead && !x.IsArchived)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(x => x.IsArchived, true)
                        .SetProperty(x => x.ArchivedAt, now)
                        .SetProperty(x => x.ArchivedBy, userId.Value),
                    cancellationToken);

            return None.Value;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error in ArchiveAllReadNotificationsHandler for User: {UserId}", request.UserId);
            return NotificationErrorDetail.ArchiveError.ArchiveFailed().ToErrorDetailResponse();
        }
    }
}
