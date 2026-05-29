using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands.CommandFlow.CommandOneFlow;
using Anemoi.BuildingBlock.Application.Results;
using Anemoi.BuildingBlock.Infrastructure.RequestHandlers.Commands.EntityFramework.EfCommandOne;
using Anemoi.Contract.Notification.Commands.NotificationCommands.MarkAsRead;
using Anemoi.Contract.Notification.Errors;
using Anemoi.Notification.Domain.Models;
using Serilog;
using System;

namespace Anemoi.Notification.Application.Cqrs.Commands.NotificationCommands.MarkAsRead;

public sealed class MarkNotificationAsReadHandler(
    ISqlRepository<NotificationHistory> sqlRepository,
    IUnitOfWork unitOfWork,
    ILogger logger)
    : EfCommandOneVoidHandler<NotificationHistory, MarkNotificationAsReadCommand>(sqlRepository, unitOfWork, logger)
{
    protected override ICommandOneFlowBuilderVoid<NotificationHistory> BuildCommand(
        IStartOneCommandVoid<NotificationHistory> fromFlow, MarkNotificationAsReadCommand command,
        CancellationToken cancellationToken)
    {
        var userGuid = Guid.Parse(command.UserId);
        return fromFlow
            .UpdateOne(x => x.Id == command.Id && x.UserId == userGuid)
            .WithSpecialAction(null)
            .WithCondition(_ => None.Value)
            .WithModify(history =>
            {
                history.IsRead = true;
                history.ReadTime = DateTime.UtcNow;
            })
            .WithErrorIfNull(NotificationErrorDetail.NotificationError.NotFound())
            .WithErrorIfSaveChange(NotificationErrorDetail.NotificationError.UpdateFailed());
    }
}
