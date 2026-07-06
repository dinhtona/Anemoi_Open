using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Contract.Notification.Responses;

namespace Anemoi.Contract.Notification.Queries.NotificationPreferenceQueries.GetMyNotificationPreference;

public sealed record GetMyNotificationPreferenceQuery(string UserId) : IQueryOne<NotificationPreferenceResponse>;
