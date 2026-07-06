using Anemoi.BuildingBlock.Application.Errors;

namespace Anemoi.Contract.Workspace.Errors;

public static class WorkspaceErrorDetail
{
    public static class WorkspaceError
    {
        public static ErrorDetail CreateFailed() => ErrorDetail.FromCode("WKE_01");

        public static ErrorDetail UpdateFailed() => ErrorDetail.FromCode("WKE_02");

        public static ErrorDetail NotFound() => ErrorDetail.FromCode("WKE_03");

        public static ErrorDetail AlreadyExist() => ErrorDetail.FromCode("WKE_04");

        public static ErrorDetail DomainExist() => ErrorDetail.FromCode("WKE_05");

        public static ErrorDetail WorkspaceExceededLimit() => ErrorDetail.FromCode("WKE_06");
    }

    public static class OrganizationError
    {
        public static ErrorDetail CreateFailed() => ErrorDetail.FromCode("OGE_01");

        public static ErrorDetail UpdateFailed() => ErrorDetail.FromCode("OGE_02");

        public static ErrorDetail NotFound() => ErrorDetail.FromCode("OGE_03");

        public static ErrorDetail AlreadyExist() => ErrorDetail.FromCode("OGE_06");
        public static ErrorDetail DomainExist() => ErrorDetail.FromCode("OGE_07");
    }

    public static class MemberInvitationError
    {
        public static ErrorDetail CreateFailed() => ErrorDetail.FromCode("MIE_01");

        public static ErrorDetail UpdateFailed() => ErrorDetail.FromCode("MIE_02");

        public static ErrorDetail NotFound() => ErrorDetail.FromCode("MIE_03");

        public static ErrorDetail AlreadyExist() => ErrorDetail.FromCode("MIE_04");

        public static ErrorDetail RemoveFailed() => ErrorDetail.FromCode("MIE_05");
    }

    public static class MemberError
    {
        public static ErrorDetail CreateFailed() => ErrorDetail.FromCode("MME_01");

        public static ErrorDetail UpdateFailed() => ErrorDetail.FromCode("MME_02");

        public static ErrorDetail NotFound() => ErrorDetail.FromCode("MME_03");

        public static ErrorDetail AlreadyExist() => ErrorDetail.FromCode("MME_04");

        public static ErrorDetail RemoveFailed() => ErrorDetail.FromCode("MME_05");
        public static ErrorDetail UserNotFound() => ErrorDetail.FromCode("MME_06");
    }
    public static class MemberMapRoleGroupError
    {
        public static ErrorDetail CreateFailed() => ErrorDetail.FromCode("MMG_01");

        public static ErrorDetail UpdateFailed() => ErrorDetail.FromCode("MMG_02");

        public static ErrorDetail NotFound() => ErrorDetail.FromCode("MMG_03");

        public static ErrorDetail RemoveFailed() => ErrorDetail.FromCode("MMG_04");
    }
}