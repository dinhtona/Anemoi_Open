using System;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Application.Results;
using Anemoi.Contract.Identity.ModelIds;
using Anemoi.Contract.Notification.Commands.NotificationPreferenceCommands.CreateOrUpdateNotificationPreference;
using Anemoi.Contract.Notification.Errors;
using Anemoi.Contract.Notification.ModelIds;
using Anemoi.Notification.Domain.Models;
using MediatR;
using OneOf;
using Serilog;

namespace Anemoi.Notification.Application.Cqrs.Commands.NotificationPreferenceCommands.CreateOrUpdateNotificationPreference;

public sealed class CreateOrUpdateNotificationPreferenceHandler(
    ISqlRepository<NotificationPreference> sqlRepository,
    IUnitOfWork unitOfWork,
    ILogger logger)
    : IRequestHandler<CreateOrUpdateNotificationPreferenceCommand, OneOf<None, ErrorDetailResponse>>
{
    public async Task<OneOf<None, ErrorDetailResponse>> Handle(
        CreateOrUpdateNotificationPreferenceCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = new UserId(Guid.Parse(request.UserId));
            var existing = await sqlRepository
                .GetFirstByConditionAsync(x => x.UserId == userId, token: cancellationToken);

            if (existing != null)
            {
                existing.EnableInApp = request.EnableInApp;
                existing.EnableEmail = request.EnableEmail;
                existing.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                var preference = new NotificationPreference
                {
                    Id = new NotificationPreferenceId(IdGenerator.NextGuid()),
                    UserId = userId,
                    EnableInApp = request.EnableInApp,
                    EnableEmail = request.EnableEmail,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                var createResult = await sqlRepository.CreateOneAsync(preference, cancellationToken);
                var isFailed = createResult.Match(_ => false, _ => true);
                if (isFailed)
                    return NotificationErrorDetail.PreferenceError.SaveFailed().ToErrorDetailResponse();
            }

            var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
            return saveResult.Match<OneOf<None, ErrorDetailResponse>>(
                _ => None.Value,
                ex =>
                {
                    logger.Error(ex, "Failed to save notification preference for User: {UserId}", request.UserId);
                    return NotificationErrorDetail.PreferenceError.SaveFailed().ToErrorDetailResponse();
                }
            );
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error in CreateOrUpdateNotificationPreferenceHandler for User: {UserId}", request.UserId);
            return NotificationErrorDetail.PreferenceError.SaveFailed().ToErrorDetailResponse();
        }
    }
}
