using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Contract.Notification.Commands.NotificationCommands.CreateNotification;
using Anemoi.Contract.Notification.Errors;
using Anemoi.Contract.Notification.Events;
using Anemoi.Contract.Identity.ModelIds;
using Anemoi.Contract.Notification.ModelIds;
using Anemoi.Contract.Notification.Responses;
using Anemoi.Notification.Application.Mappings;
using Anemoi.Notification.Domain.Models;
using MassTransit;
using MediatR;
using OneOf;
using Serilog;

namespace Anemoi.Notification.Application.Cqrs.Commands.NotificationCommands.CreateNotification;

public sealed class CreateNotificationHandler(
    ISqlRepository<NotificationHistory> sqlRepository,
    ISqlRepository<NotificationSubscription> subscriptionRepository,
    ISqlRepository<NotificationPreference> preferenceRepository,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publishEndpoint,
    NotificationMapper mapper,
    ILogger logger)
    : IRequestHandler<CreateNotificationCommand, OneOf<NotificationResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<NotificationResponse, ErrorDetailResponse>> Handle(CreateNotificationCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userGuid = Guid.Parse(request.UserId);
            
            // Check global in-app preference
            var preference = await preferenceRepository.GetFirstByConditionAsync(
                x => x.UserId == new UserId(userGuid), token: cancellationToken);
            if (preference != null && !preference.EnableInApp)
            {
                logger.Information(
                    "In-app notifications disabled for User {UserId}. Skipping creation.",
                    request.UserId);
                return new NotificationResponse
                {
                    Id = Guid.Empty.ToString(),
                    UserId = request.UserId,
                    Title = request.Title,
                    Content = request.Content,
                    Category = request.Category,
                    IsRead = true,
                    CreatedTime = DateTime.UtcNow
                };
            }

            // Check if user has disabled this category
            var subscription = await subscriptionRepository.GetFirstByConditionAsync(
                x => x.UserId == new UserId(userGuid) && x.Category == request.Category,
                token: cancellationToken);

            if (subscription != null && !subscription.IsEnabled)
            {
                // User has explicitly unsubscribed/disabled this category, so skip sending/saving
                logger.Information("Notification category {Category} is disabled for User {UserId}. Skipping creation.", request.Category, request.UserId);
                return new NotificationResponse
                {
                    Id = Guid.Empty.ToString(),
                    UserId = request.UserId,
                    Title = request.Title,
                    Content = request.Content,
                    Category = request.Category,
                    IsRead = true,
                    CreatedTime = DateTime.UtcNow
                };
            }

            // Deduplication Check
            if (!string.IsNullOrEmpty(request.DeduplicationKey))
            {
                var existing = await sqlRepository.GetFirstByConditionAsync(
                    x => x.UserId == new UserId(userGuid) && x.DeduplicationKey == request.DeduplicationKey,
                    token: cancellationToken);

                if (existing != null)
                {
                    logger.Information("Duplicate notification detected via check for DeduplicationKey {DeduplicationKey} for User {UserId}. Skipping creation.", request.DeduplicationKey, request.UserId);
                    return mapper.ToNotificationResponse(existing);
                }
            }

            // Create notification record
            var notification = new NotificationHistory
            {
                Id = new NotificationHistoryId(IdGenerator.NextGuid()),
                UserId = new UserId(userGuid),
                WorkspaceId = request.WorkspaceId is { } ws ? Guid.Parse(ws) : null,
                Title = request.Title,
                Content = request.Content,
                Category = request.Category,
                CreatedTime = DateTime.UtcNow,
                TitleLocalizationKey = request.TitleLocalizationKey,
                TitleLocalizationArgs = request.TitleLocalizationArgs == null ? null : new List<string>(request.TitleLocalizationArgs),
                ContentLocalizationKey = request.ContentLocalizationKey,
                ContentLocalizationArgs = request.ContentLocalizationArgs == null ? null : new List<string>(request.ContentLocalizationArgs),
                ActionUrl = request.ActionUrl,
                ActionType = request.ActionType,
                DeduplicationKey = request.DeduplicationKey,
                CorrelationId = request.CorrelationId,
                CausationId = request.CausationId,
                Type = string.IsNullOrEmpty(request.Type) ? Anemoi.Contract.Notification.Constants.NotificationConstants.Types.Business : request.Type,
                Severity = string.IsNullOrEmpty(request.Severity) ? Anemoi.Contract.Notification.Constants.NotificationConstants.Severities.Info : request.Severity
            };

            var createResult = await sqlRepository.CreateOneAsync(notification, cancellationToken);
            var isFailed = createResult.Match(_ => false, _ => true);
            if (isFailed)
            {
                return NotificationErrorDetail.NotificationError.CreateFailed().ToErrorDetailResponse();
            }

            var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
            return await saveResult.Match<Task<OneOf<NotificationResponse, ErrorDetailResponse>>>(
                async _ =>
                {
                    // Publish Integration Event for Gateway SignalR push
                    await publishEndpoint.Publish(new NotificationCreatedIntegrationEvent
                    {
                        Id = notification.Id.Value.ToString(),
                        UserId = notification.UserId.ToString(),
                        WorkspaceId = notification.WorkspaceId?.ToString(),
                        Title = notification.Title,
                        Content = notification.Content,
                        Category = notification.Category,
                        CreatedTime = notification.CreatedTime,
                        TitleLocalizationKey = notification.TitleLocalizationKey,
                        TitleLocalizationArgs = notification.TitleLocalizationArgs,
                        ContentLocalizationKey = notification.ContentLocalizationKey,
                        ContentLocalizationArgs = notification.ContentLocalizationArgs,
                        ActionUrl = notification.ActionUrl,
                        ActionType = notification.ActionType,
                        DeduplicationKey = notification.DeduplicationKey,
                        CorrelationId = notification.CorrelationId,
                        CausationId = notification.CausationId,
                        Type = notification.Type,
                        Severity = notification.Severity
                    }, cancellationToken);

                    logger.Information("Successfully created notification {NotificationId} of category {Category} for User {UserId}", notification.Id, notification.Category, request.UserId);
                    return mapper.ToNotificationResponse(notification);
                },
                async ex =>
                {
                    // Handle concurrent insert unique key race conditions
                    if (!string.IsNullOrEmpty(request.DeduplicationKey))
                    {
                        var existing = await sqlRepository.GetFirstByConditionAsync(
                            x => x.UserId == new UserId(userGuid) && x.DeduplicationKey == request.DeduplicationKey,
                            token: cancellationToken);
                        if (existing != null)
                        {
                            logger.Information("Recovered from concurrent insert for DeduplicationKey {DeduplicationKey} in SaveChangesAsync.", request.DeduplicationKey);
                            return mapper.ToNotificationResponse(existing);
                        }
                    }
                    logger.Error(ex, "Error occurred while saving new notification for User: {UserId}", request.UserId);
                    return NotificationErrorDetail.NotificationError.CreateFailed().ToErrorDetailResponse();
                }
            );
        }
        catch (Exception ex)
        {
            if (!string.IsNullOrEmpty(request.DeduplicationKey))
            {
                try
                {
                    var userGuid = Guid.Parse(request.UserId);
                    var existing = await sqlRepository.GetFirstByConditionAsync(
                        x => x.UserId == new UserId(userGuid) && x.DeduplicationKey == request.DeduplicationKey,
                        token: cancellationToken);
                    if (existing != null)
                    {
                        logger.Information("Recovered from concurrent exception for DeduplicationKey {DeduplicationKey} for User {UserId}.", request.DeduplicationKey, request.UserId);
                        return mapper.ToNotificationResponse(existing);
                    }
                }
                catch
                {
                    // Ignore nested error to preserve original exception logging
                }
            }
            logger.Error(ex, "Error occurred in CreateNotificationHandler for User: {UserId}", request.UserId);
            return NotificationErrorDetail.NotificationError.CreateFailed().ToErrorDetailResponse();
        }
    }
}
