using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Authorization;
using Anemoi.Contract.Hr.Events;
using Anemoi.Contract.Identity.Queries.UserQueries.ResolveUsersByPermission;
using Anemoi.Contract.Notification.Commands.NotificationCommands.CreateNotification;
using Anemoi.Contract.Notification.Constants;
using Anemoi.Contract.Notification.Events;
using MassTransit;
using MediatR;
using Serilog;

namespace Anemoi.Notification.Application.Consumers;

public sealed class PayrollRunSubmittedConsumer(
    IRequestClient<ResolveUsersByPermissionQuery> permissionClient,
    IMediator mediator,
    IPublishEndpoint publishEndpoint,
    ILogger logger)
    : IConsumer<PayrollRunSubmittedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<PayrollRunSubmittedIntegrationEvent> context)
    {
        var message = context.Message;

        // Emit DataChangeOccurred transient invalidation event first (always emit!)
        await publishEndpoint.Publish(new DataChangeOccurredIntegrationEvent
        {
            Resource = "hr.payroll",
            Action = NotificationConstants.DataChangeActions.Update,
            EntityId = message.PayrollRunId,
            WorkspaceId = null,
            Sensitivity = NotificationConstants.DataSensitivity.Low,
            QueryTags = new List<string> { "hr", "payroll" },
            OccurredAt = DateTime.UtcNow
        }, context.CancellationToken);

        var targetPermission = Permissions.HrPayrollApprove;

        try
        {
            var response = await permissionClient.GetResponse<ResolveUsersByPermissionResponse>(
                new ResolveUsersByPermissionQuery(targetPermission), context.CancellationToken);

            var resolvedUsers = response.Message?.Users ?? Array.Empty<PermissionUser>();

            // Exclude the submitter and deduplicate by UserId
            var targetUserIds = resolvedUsers
                .Select(u => u.UserId)
                .Where(id => !string.IsNullOrEmpty(id) && id != message.SubmittedBy)
                .Distinct()
                .ToList();

            if (targetUserIds.Count == 0)
            {
                logger.Warning("Empty permission audience for PayrollRunSubmitted. No users have permission {Permission}.", targetPermission);
            }

            foreach (var userId in targetUserIds)
            {
                var command = new CreateNotificationCommand(
                    UserId: userId,
                    TitleLocalizationKey: "notification.payroll.submitted.title",
                    ContentLocalizationKey: "notification.payroll.submitted.content",
                    Category: NotificationConstants.Categories.Payroll,
                    ActionUrl: "/hr/payroll",
                    DeduplicationKey: $"payroll:{message.PayrollRunId}:submitted:{userId}",
                    Type: NotificationConstants.Types.Business,
                    Severity: NotificationConstants.Severities.Info
                );
                await mediator.Send(command, context.CancellationToken);
            }

            logger.Information("Processed PayrollRunSubmitted notifications. Dispatched to {Count} users.", targetUserIds.Count);
        }
        catch (RequestTimeoutException ex)
        {
            logger.Warning(ex, "Request timeout when resolving users with permission {Permission}. Skipping business notifications.", targetPermission);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error resolving users with permission {Permission}. Skipping business notifications.", targetPermission);
        }
    }
}

public sealed class PayrollRunApprovedConsumer(
    IRequestClient<ResolveUsersByPermissionQuery> permissionClient,
    IMediator mediator,
    IPublishEndpoint publishEndpoint,
    ILogger logger)
    : IConsumer<PayrollRunApprovedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<PayrollRunApprovedIntegrationEvent> context)
    {
        var message = context.Message;

        // Emit DataChangeOccurred transient invalidation event first (always emit!)
        await publishEndpoint.Publish(new DataChangeOccurredIntegrationEvent
        {
            Resource = "hr.payroll",
            Action = NotificationConstants.DataChangeActions.Update,
            EntityId = message.PayrollRunId,
            WorkspaceId = null,
            Sensitivity = NotificationConstants.DataSensitivity.Low,
            QueryTags = new List<string> { "hr", "payroll" },
            OccurredAt = DateTime.UtcNow
        }, context.CancellationToken);

        var targetPermission = Permissions.HrPayrollView;

        try
        {
            var response = await permissionClient.GetResponse<ResolveUsersByPermissionResponse>(
                new ResolveUsersByPermissionQuery(targetPermission), context.CancellationToken);

            var resolvedUsers = response.Message?.Users ?? Array.Empty<PermissionUser>();

            // Exclude actor and deduplicate
            var targetUserIds = resolvedUsers
                .Select(u => u.UserId)
                .Where(id => !string.IsNullOrEmpty(id) && id != message.ApprovedBy)
                .Distinct()
                .ToList();

            if (targetUserIds.Count == 0)
            {
                logger.Warning("Empty permission audience for PayrollRunApproved. No users have permission {Permission}.", targetPermission);
            }

            foreach (var userId in targetUserIds)
            {
                var command = new CreateNotificationCommand(
                    UserId: userId,
                    TitleLocalizationKey: "notification.payroll.approved.title",
                    ContentLocalizationKey: "notification.payroll.approved.content",
                    Category: NotificationConstants.Categories.Payroll,
                    ActionUrl: "/hr/payroll",
                    DeduplicationKey: $"payroll:{message.PayrollRunId}:approved:{userId}",
                    Type: NotificationConstants.Types.Business,
                    Severity: NotificationConstants.Severities.Info
                );
                await mediator.Send(command, context.CancellationToken);
            }

            logger.Information("Processed PayrollRunApproved notifications. Dispatched to {Count} users.", targetUserIds.Count);
        }
        catch (RequestTimeoutException ex)
        {
            logger.Warning(ex, "Request timeout when resolving users with permission {Permission}. Skipping business notifications.", targetPermission);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error resolving users with permission {Permission}. Skipping business notifications.", targetPermission);
        }
    }
}

public sealed class PayrollRunRejectedConsumer(
    IRequestClient<ResolveUsersByPermissionQuery> permissionClient,
    IMediator mediator,
    IPublishEndpoint publishEndpoint,
    ILogger logger)
    : IConsumer<PayrollRunRejectedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<PayrollRunRejectedIntegrationEvent> context)
    {
        var message = context.Message;

        // Emit DataChangeOccurred transient invalidation event first (always emit!)
        await publishEndpoint.Publish(new DataChangeOccurredIntegrationEvent
        {
            Resource = "hr.payroll",
            Action = NotificationConstants.DataChangeActions.Update,
            EntityId = message.PayrollRunId,
            WorkspaceId = null,
            Sensitivity = NotificationConstants.DataSensitivity.Low,
            QueryTags = new List<string> { "hr", "payroll" },
            OccurredAt = DateTime.UtcNow
        }, context.CancellationToken);

        var targetPermission = Permissions.HrPayrollView;

        try
        {
            var response = await permissionClient.GetResponse<ResolveUsersByPermissionResponse>(
                new ResolveUsersByPermissionQuery(targetPermission), context.CancellationToken);

            var resolvedUsers = response.Message?.Users ?? Array.Empty<PermissionUser>();

            // Exclude actor and deduplicate
            var targetUserIds = resolvedUsers
                .Select(u => u.UserId)
                .Where(id => !string.IsNullOrEmpty(id) && id != message.RejectedBy)
                .Distinct()
                .ToList();

            if (targetUserIds.Count == 0)
            {
                logger.Warning("Empty permission audience for PayrollRunRejected. No users have permission {Permission}.", targetPermission);
            }

            foreach (var userId in targetUserIds)
            {
                var command = new CreateNotificationCommand(
                    UserId: userId,
                    TitleLocalizationKey: "notification.payroll.rejected.title",
                    ContentLocalizationKey: "notification.payroll.rejected.content",
                    Category: NotificationConstants.Categories.Payroll,
                    ActionUrl: "/hr/payroll",
                    DeduplicationKey: $"payroll:{message.PayrollRunId}:rejected:{userId}",
                    Type: NotificationConstants.Types.Business,
                    Severity: NotificationConstants.Severities.Info
                );
                await mediator.Send(command, context.CancellationToken);
            }

            logger.Information("Processed PayrollRunRejected notifications. Dispatched to {Count} users.", targetUserIds.Count);
        }
        catch (RequestTimeoutException ex)
        {
            logger.Warning(ex, "Request timeout when resolving users with permission {Permission}. Skipping business notifications.", targetPermission);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error resolving users with permission {Permission}. Skipping business notifications.", targetPermission);
        }
    }
}

public sealed class PayrollRunFinalizedConsumer(
    IRequestClient<ResolveUsersByPermissionQuery> permissionClient,
    IMediator mediator,
    IPublishEndpoint publishEndpoint,
    ILogger logger)
    : IConsumer<PayrollRunFinalizedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<PayrollRunFinalizedIntegrationEvent> context)
    {
        var message = context.Message;

        // Emit DataChangeOccurred transient invalidation event first (always emit!)
        await publishEndpoint.Publish(new DataChangeOccurredIntegrationEvent
        {
            Resource = "hr.payroll",
            Action = NotificationConstants.DataChangeActions.Update,
            EntityId = message.PayrollRunId,
            WorkspaceId = null,
            Sensitivity = NotificationConstants.DataSensitivity.Low,
            QueryTags = new List<string> { "hr", "payroll" },
            OccurredAt = DateTime.UtcNow
        }, context.CancellationToken);

        var targetPermission = Permissions.HrPayrollView;

        try
        {
            var response = await permissionClient.GetResponse<ResolveUsersByPermissionResponse>(
                new ResolveUsersByPermissionQuery(targetPermission), context.CancellationToken);

            var resolvedUsers = response.Message?.Users ?? Array.Empty<PermissionUser>();

            // Exclude actor and deduplicate
            var targetUserIds = resolvedUsers
                .Select(u => u.UserId)
                .Where(id => !string.IsNullOrEmpty(id) && id != message.FinalizedBy)
                .Distinct()
                .ToList();

            if (targetUserIds.Count == 0)
            {
                logger.Warning("Empty permission audience for PayrollRunFinalized. No users have permission {Permission}.", targetPermission);
            }

            foreach (var userId in targetUserIds)
            {
                var command = new CreateNotificationCommand(
                    UserId: userId,
                    TitleLocalizationKey: "notification.payroll.finalized.title",
                    ContentLocalizationKey: "notification.payroll.finalized.content",
                    Category: NotificationConstants.Categories.Payroll,
                    ActionUrl: "/hr/payroll",
                    DeduplicationKey: $"payroll:{message.PayrollRunId}:finalized:{userId}",
                    Type: NotificationConstants.Types.Business,
                    Severity: NotificationConstants.Severities.Info
                );
                await mediator.Send(command, context.CancellationToken);
            }

            logger.Information("Processed PayrollRunFinalized notifications. Dispatched to {Count} users.", targetUserIds.Count);
        }
        catch (RequestTimeoutException ex)
        {
            logger.Warning(ex, "Request timeout when resolving users with permission {Permission}. Skipping business notifications.", targetPermission);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error resolving users with permission {Permission}. Skipping business notifications.", targetPermission);
        }
    }
}
