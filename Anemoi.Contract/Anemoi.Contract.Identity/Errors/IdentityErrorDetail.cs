using Anemoi.BuildingBlock.Application.Errors;

namespace Anemoi.Contract.Identity.Errors;

public static class IdentityErrorDetail
{
    public static class IdentityError
    {
        public static ErrorDetail UserLockedOut() => ErrorDetail.FromCode("IDE_01");

        public static ErrorDetail UserLogInNotAllowed() => ErrorDetail.FromCode("IDE_02");

        public static ErrorDetail UserRequiresTwoFactor() => ErrorDetail.FromCode("IDE_03");

        public static ErrorDetail UserLogOutFailed() => ErrorDetail.FromCode("IDE_04");

        public static ErrorDetail FailedToChangePassword() => ErrorDetail.FromCode("IDE_05");

        public static ErrorDetail PasswordNotCorrect() => ErrorDetail.FromCode("IDE_06");

        public static ErrorDetail LoginFailed() => ErrorDetail.FromCode("IDE_07");

        public static ErrorDetail LockUserFailed() => ErrorDetail.FromCode("IDE_08");

        public static ErrorDetail ResetUserPasswordFailed() => ErrorDetail.FromCode("IDE_09");

        public static ErrorDetail FirstTimePasswordWasNotChanged() => ErrorDetail.FromCode("IDE_10");

        public static ErrorDetail UserEmailWasNotVerified() => ErrorDetail.FromCode("IDE_12");

        public static ErrorDetail PasswordSuccessRehashNeeded() => ErrorDetail.FromCode("IDE_15");
    }

    public static class TokenError
    {
        public static ErrorDetail RefreshTokenUsed() => ErrorDetail.FromCode("TEE_01");

        public static ErrorDetail TokenIsNotExpired() => ErrorDetail.FromCode("TEE_02");

        public static ErrorDetail RefreshTokenNotFound() => ErrorDetail.FromCode("TEE_03");

        public static ErrorDetail CreateRefreshTokenFailed() => ErrorDetail.FromCode("TEE_04");

        public static ErrorDetail InvalidToken() => ErrorDetail.FromCode("TEE_05");

        public static ErrorDetail SessionExpired() => ErrorDetail.FromCode("TEE_07");

        public static ErrorDetail InvalidRefreshToken() => ErrorDetail.FromCode("TEE_08");
        public static ErrorDetail TokenExpired() => ErrorDetail.FromCode("TEE_09");
    }

    public static class UserError
    {
        public static ErrorDetail NotFound() => ErrorDetail.FromCode("AUE_01");

        public static ErrorDetail CreateFailed() => ErrorDetail.FromCode("AUE_02");

        public static ErrorDetail UpdateFailed() => ErrorDetail.FromCode("AUE_03");

        public static ErrorDetail RemoveFailed() => ErrorDetail.FromCode("AUE_04");

        public static ErrorDetail EmailConfirmed() => ErrorDetail.FromCode("AUE_05");

        public static ErrorDetail UserExisted() => ErrorDetail.FromCode("AUE_06");

        public static ErrorDetail PasswordNotValid() => ErrorDetail.FromCode("AUE_11");

        public static ErrorDetail AlreadyAdministrator() => ErrorDetail.FromCode("AUE_12");

        public static ErrorDetail NotAdministrator() => ErrorDetail.FromCode("AUE_13");

        public static ErrorDetail CannotDemoteSelf() => ErrorDetail.FromCode("AUE_14");

        public static ErrorDetail CannotDemoteLastAdministrator() => ErrorDetail.FromCode("AUE_15");
    }

    public static class RoleError
    {
        public static ErrorDetail RolesRequestDuplicated() => ErrorDetail.FromCode("ROE_01");

        public static ErrorDetail RolesNotExist() => ErrorDetail.FromCode("ROE_02");

        public static ErrorDetail RemoveRolesError() => ErrorDetail.FromCode("ROE_03");

        public static ErrorDetail AddRolesError() => ErrorDetail.FromCode("ROE_04");

        public static ErrorDetail RolesRequestMustNotBeNull() => ErrorDetail.FromCode("ROE_05");

        public static ErrorDetail ReservedSystemRole() => ErrorDetail.FromCode("ROE_06");
    }

    public static class RoleGroupError
    {
        public static ErrorDetail NotFound() => ErrorDetail.FromCode("RGE_01");

        public static ErrorDetail CreateFailed() => ErrorDetail.FromCode("RGE_02");

        public static ErrorDetail UpdateFailed() => ErrorDetail.FromCode("RGE_03");

        public static ErrorDetail RemoveFailed() => ErrorDetail.FromCode("RGE_04");

        public static ErrorDetail DuplicateNameFailed() => ErrorDetail.FromCode("RGE_05");

        public static ErrorDetail Applied() => ErrorDetail.FromCode("RGE_06");

        public static ErrorDetail RoleGroupDefault() => ErrorDetail.FromCode("RGE_08");

        public static ErrorDetail WorkspaceRoleGroup() => ErrorDetail.FromCode("RGE_09");

        public static ErrorDetail WorkspaceScope() => ErrorDetail.FromCode("RGE_10");
    }

    public static class ApplicationPolicyError
    {
        public static ErrorDetail NotFound() => ErrorDetail.FromCode("APE_01");

        public static ErrorDetail CreateFailed() => ErrorDetail.FromCode("APE_02");

        public static ErrorDetail UpdateFailed() => ErrorDetail.FromCode("APE_03");

        public static ErrorDetail RemoveFailed() => ErrorDetail.FromCode("APE_04");
    }

    public static class UserMapRoleGroupError
    {
        public static ErrorDetail NotFound() => ErrorDetail.FromCode("UMG_01");

        public static ErrorDetail CreateFailed() => ErrorDetail.FromCode("UMG_02");

        public static ErrorDetail UpdateFailed() => ErrorDetail.FromCode("UMG_03");

        public static ErrorDetail RemoveFailed() => ErrorDetail.FromCode("UMG_04");

        public static ErrorDetail RoleGroupsRequestDuplicated() => ErrorDetail.FromCode("UMG_05");
    }
}
