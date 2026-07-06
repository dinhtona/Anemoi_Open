using Anemoi.BuildingBlock.Application.Errors;

namespace Anemoi.Contract.Notification.Errors;

public static class NotificationErrorDetail
{
    public static class NotificationError
    {
        public static ErrorDetail CreateFailed() => ErrorDetail.FromCode("NTE_01");

        public static ErrorDetail UpdateFailed() => ErrorDetail.FromCode("NTE_02");

        public static ErrorDetail NotFound() => ErrorDetail.FromCode("NTE_03");

        public static ErrorDetail HideFailed() => ErrorDetail.FromCode("NTE_04");
    }

    public static class SettingError
    {
        public static ErrorDetail SaveFailed() => ErrorDetail.FromCode("NSE_01");
    }

    public static class PreferenceError
    {
        public static ErrorDetail SaveFailed() => ErrorDetail.FromCode("NPE_01");
    }

    public static class ActionError
    {
        public static ErrorDetail ActionNotFound() => ErrorDetail.FromCode("NAE_01");
        public static ErrorDetail NotificationNotFound() => ErrorDetail.FromCode("NAE_02");
        public static ErrorDetail NotificationHidden() => ErrorDetail.FromCode("NAE_03");
        public static ErrorDetail ActionExecutionFailed() => ErrorDetail.FromCode("NAE_04");
        public static ErrorDetail ExecutorNotFound() => ErrorDetail.FromCode("NAE_05");
        public static ErrorDetail OwnershipMismatch() => ErrorDetail.FromCode("NAE_06");
        public static ErrorDetail NotificationArchived() => ErrorDetail.FromCode("NAE_07");
    }

    public static class ArchiveError
    {
        public static ErrorDetail ArchiveFailed() => ErrorDetail.FromCode("NAR_01");
        public static ErrorDetail UnarchiveFailed() => ErrorDetail.FromCode("NAR_02");
        public static ErrorDetail AlreadyArchived() => ErrorDetail.FromCode("NAR_03");
        public static ErrorDetail NotArchived() => ErrorDetail.FromCode("NAR_04");
    }
}
