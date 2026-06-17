using System;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Application.Results;
using Anemoi.Contract.Identity.ModelIds;
using Anemoi.Contract.Notification.ModelIds;
using Anemoi.Contract.Notification.Commands.NotificationCommands.ExecuteNotificationAction;
using Anemoi.Contract.Notification.Errors;
using Anemoi.Notification.Application.Services;
using Anemoi.Notification.Domain.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;
using Serilog;

namespace Anemoi.Notification.Application.Cqrs.Commands.NotificationCommands.ExecuteNotificationAction;

public sealed class ExecuteNotificationActionHandler(
    ISqlRepository<NotificationHistory> sqlRepository,
    ISqlRepository<NotificationActionAudit> auditRepository,
    INotificationActionExecutorResolver executorResolver,
    IUnitOfWork unitOfWork,
    ILogger logger)
    : IRequestHandler<ExecuteNotificationActionCommand, OneOf<ExecuteNotificationActionResult, ErrorDetailResponse>>
{
    public async Task<OneOf<ExecuteNotificationActionResult, ErrorDetailResponse>> Handle(
        ExecuteNotificationActionCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var notification = await sqlRepository
                .GetFirstByConditionAsync(
                    x => x.Id == request.NotificationId,
                    q => q.Include(n => n.Actions),
                    cancellationToken);

            if (notification == null)
                return NotificationErrorDetail.ActionError.NotificationNotFound().ToErrorDetailResponse();

            var userId = new UserId(Guid.Parse(request.UserId));
            if (notification.UserId != userId)
                return NotificationErrorDetail.ActionError.OwnershipMismatch().ToErrorDetailResponse();

            if (notification.IsHidden)
                return NotificationErrorDetail.ActionError.NotificationHidden().ToErrorDetailResponse();

            if (notification.IsArchived)
                return NotificationErrorDetail.ActionError.NotificationArchived().ToErrorDetailResponse();

            var action = notification.Actions.FirstOrDefault(a => a.Id == request.ActionId);

            if (action == null)
                return NotificationErrorDetail.ActionError.ActionNotFound().ToErrorDetailResponse();

            var executor = executorResolver.Resolve(action.ActionCode);
            if (executor == null)
                return NotificationErrorDetail.ActionError.ExecutorNotFound().ToErrorDetailResponse();

            var executionResult = await executor.ExecuteAsync(notification, action, cancellationToken);

            return await executionResult.Match<Task<OneOf<ExecuteNotificationActionResult, ErrorDetailResponse>>>(
                async result =>
                {
                    var audit = new NotificationActionAudit
                    {
                        Id = new NotificationActionAuditId(IdGenerator.NextGuid()),
                        NotificationId = request.NotificationId,
                        ActionId = request.ActionId,
                        ExecutedBy = userId,
                        ExecutedAt = DateTime.UtcNow,
                        Success = result.Success,
                        Result = result.Message,
                        ClientIp = request.ClientIp,
                        UserAgent = request.UserAgent
                    };

                    await auditRepository.CreateOneAsync(audit, cancellationToken);
                    var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);

                    if (saveResult.IsT1)
                    {
                        logger.Error(saveResult.AsT1, "Failed to save audit record for action execution");
                        return NotificationErrorDetail.ActionError.ActionExecutionFailed().ToErrorDetailResponse();
                    }

                    return new ExecuteNotificationActionResult(
                        result.ActionType,
                        result.TargetUrl,
                        result.Success,
                        result.Message
                    );
                },
                async error =>
                {
                    var audit = new NotificationActionAudit
                    {
                        Id = new NotificationActionAuditId(IdGenerator.NextGuid()),
                        NotificationId = request.NotificationId,
                        ActionId = request.ActionId,
                        ExecutedBy = userId,
                        ExecutedAt = DateTime.UtcNow,
                        Success = false,
                        Result = string.Join("; ", error.Messages ?? []),
                        ClientIp = request.ClientIp,
                        UserAgent = request.UserAgent
                    };

                    await auditRepository.CreateOneAsync(audit, cancellationToken);
                    await unitOfWork.SaveChangesAsync(cancellationToken);

                    return error;
                }
            );
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error executing notification action {ActionId} for notification {NotificationId}",
                request.ActionId, request.NotificationId);
            return NotificationErrorDetail.ActionError.ActionExecutionFailed().ToErrorDetailResponse();
        }
    }
}
