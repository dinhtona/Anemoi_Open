using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Results;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Contract.Notification.Commands.NotificationSettingsCommands.UpdateNotificationSettings;
using Anemoi.Contract.Notification.Errors;
using Anemoi.Contract.Notification.ModelIds;
using Anemoi.Notification.Domain.Models;
using MediatR;
using OneOf;
using Serilog;

namespace Anemoi.Notification.Application.Cqrs.Commands.NotificationSettingsCommands.UpdateNotificationSettings;

public sealed class UpdateNotificationSettingsHandler(
    ISqlRepository<NotificationSubscription> sqlRepository,
    IUnitOfWork unitOfWork,
    ILogger logger)
    : IRequestHandler<UpdateNotificationSettingsCommand, OneOf<None, ErrorDetailResponse>>
{
    public async Task<OneOf<None, ErrorDetailResponse>> Handle(UpdateNotificationSettingsCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userGuid = Guid.Parse(request.UserId);
            var existingSettings = await sqlRepository
                .GetManyByConditionAsync(x => x.UserId == userGuid, token: cancellationToken);

            var toCreate = new List<NotificationSubscription>();

            foreach (var setting in request.Settings)
            {
                var existing = existingSettings.FirstOrDefault(x => x.Category.Equals(setting.Category, StringComparison.OrdinalIgnoreCase));
                if (existing != null)
                {
                    existing.SetEnabled(setting.IsEnabled);
                }
                else
                {
                    var sub = new NotificationSubscription
                    {
                        Id = new NotificationSubscriptionId(IdGenerator.NextGuid()),
                        UserId = userGuid,
                        Category = setting.Category
                    };
                    sub.SetEnabled(setting.IsEnabled);
                    toCreate.Add(sub);
                }
            }

            if (toCreate.Any())
            {
                var createResult = await sqlRepository.CreateManyAsync(toCreate, cancellationToken);
                var isFailed = createResult.Match(_ => false, _ => true);
                if (isFailed)
                {
                    return NotificationErrorDetail.SettingError.SaveFailed().ToErrorDetailResponse();
                }
            }

            var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
            return saveResult.Match<OneOf<None, ErrorDetailResponse>>(
                _ => None.Value,
                ex =>
                {
                    logger.Error(ex, "Error occurred while saving notification settings for User: {UserId}", request.UserId);
                    return NotificationErrorDetail.SettingError.SaveFailed().ToErrorDetailResponse();
                }
            );
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error occurred in UpdateNotificationSettingsHandler for User: {UserId}", request.UserId);
            return NotificationErrorDetail.SettingError.SaveFailed().ToErrorDetailResponse();
        }
    }
}
