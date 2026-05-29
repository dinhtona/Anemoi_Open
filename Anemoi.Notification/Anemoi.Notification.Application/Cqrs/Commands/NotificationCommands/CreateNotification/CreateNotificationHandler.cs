using System;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Contract.Notification.Commands.NotificationCommands.CreateNotification;
using Anemoi.Contract.Notification.Errors;
using Anemoi.Contract.Notification.Events;
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
            
            // Check if user has disabled this category
            var subscription = await subscriptionRepository.GetFirstByConditionAsync(
                x => x.UserId == userGuid && x.Category == request.Category,
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

            // Create notification record
            var notification = new NotificationHistory
            {
                Id = new NotificationHistoryId(IdGenerator.NextGuid()),
                UserId = userGuid,
                Title = request.Title,
                Content = request.Content,
                Category = request.Category,
                IsRead = false,
                CreatedTime = DateTime.UtcNow
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
                        Title = notification.Title,
                        Content = notification.Content,
                        Category = notification.Category,
                        CreatedTime = notification.CreatedTime
                    }, cancellationToken);

                    logger.Information("Successfully created notification {NotificationId} of category {Category} for User {UserId}", notification.Id, notification.Category, request.UserId);
                    return mapper.ToNotificationResponse(notification);
                },
                ex =>
                {
                    logger.Error(ex, "Error occurred while saving new notification for User: {UserId}", request.UserId);
                    return Task.FromResult<OneOf<NotificationResponse, ErrorDetailResponse>>(
                        NotificationErrorDetail.NotificationError.CreateFailed().ToErrorDetailResponse());
                }
            );
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error occurred in CreateNotificationHandler for User: {UserId}", request.UserId);
            return NotificationErrorDetail.NotificationError.CreateFailed().ToErrorDetailResponse();
        }
    }
}
