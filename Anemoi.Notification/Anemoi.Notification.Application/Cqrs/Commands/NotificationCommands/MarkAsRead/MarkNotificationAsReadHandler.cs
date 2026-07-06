using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands.CommandFlow.CommandOneFlow;
using Anemoi.BuildingBlock.Application.Results;
using Anemoi.BuildingBlock.Application.RequestHandlers.Commands.EntityFramework.EfCommandOne;
using Anemoi.Contract.Identity.ModelIds;
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
        var userId = new UserId(Guid.Parse(command.UserId));
        return fromFlow
            .UpdateOne(x => x.Id == command.Id && x.UserId == userId)
            .WithSpecialAction(null)
            .WithCondition(_ => None.Value)
            .WithModify(history =>
            {
                history.MarkAsRead();
            })
            .WithErrorIfNull(NotificationErrorDetail.NotificationError.NotFound())
            .WithErrorIfSaveChange(NotificationErrorDetail.NotificationError.UpdateFailed());
    }
}
