using System.Collections.Generic;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Contract.Notification.Responses;

namespace Anemoi.Contract.Notification.Commands.NotificationSettingsCommands.UpdateNotificationSettings;

public sealed record UpdateNotificationSettingsCommand(string UserId, List<NotificationSettingResponse> Settings) : ICommandVoid;
