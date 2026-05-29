using Anemoi.BuildingBlock.Application.Cqrs.Queries;

namespace Anemoi.Contract.Notification.Queries.NotificationQueries.GetUnreadNotificationCount;

public sealed record GetUnreadNotificationCountQuery(string UserId) : IQueryCounting;
