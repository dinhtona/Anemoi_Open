using Anemoi.BuildingBlock.Application.Errors;

namespace Anemoi.Contract.Notification.Errors;

public static class NotificationErrorDetail
{
    public static class NotificationError
    {
        public static ErrorDetail CreateFailed() => ErrorDetail.FromCode("NTE_01");

        public static ErrorDetail UpdateFailed() => ErrorDetail.FromCode("NTE_02");

        public static ErrorDetail NotFound() => ErrorDetail.FromCode("NTE_03");
    }

    public static class SettingError
    {
        public static ErrorDetail SaveFailed() => ErrorDetail.FromCode("NSE_01");
    }
}
