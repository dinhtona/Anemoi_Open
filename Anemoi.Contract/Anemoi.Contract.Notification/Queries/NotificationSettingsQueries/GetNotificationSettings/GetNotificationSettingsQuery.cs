using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Contract.Notification.Responses;

namespace Anemoi.Contract.Notification.Queries.NotificationSettingsQueries.GetNotificationSettings;

public sealed record GetNotificationSettingsQuery(string UserId) : IQueryCollection<NotificationSettingResponse>;
