using Anemoi.BuildingBlock.Application.Errors;

namespace Anemoi.Contract.Notification.Errors;

public static class NotificationErrorDetail
{
    public static class NotificationError
    {
        public static ErrorDetail CreateFailed() => new()
        {
            Messages = new[] { "Error while creating a new notification!" }, Code = "NTE_01"
        };

        public static ErrorDetail UpdateFailed() => new()
        {
            Messages = new[] { "Error while updating an exist notification!" }, Code = "NTE_02"
        };

        public static ErrorDetail NotFound() => new()
        {
            Messages = new[] { "Notification was not found!" }, Code = "NTE_03"
        };
    }

    public static class SettingError
    {
        public static ErrorDetail SaveFailed() => new()
        {
            Messages = new[] { "Error while saving notification settings!" }, Code = "NSE_01"
        };
    }
}
