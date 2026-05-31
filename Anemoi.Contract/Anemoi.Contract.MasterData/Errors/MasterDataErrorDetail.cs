using Anemoi.BuildingBlock.Application.Errors;

namespace Anemoi.Contract.MasterData.Errors;

public static class MasterDataErrorDetail
{
    public static class ProvinceError
    {
        public static ErrorDetail CreateFailed() => ErrorDetail.FromCode("PEE_01");

        public static ErrorDetail UpdateFailed() => ErrorDetail.FromCode("PEE_02");

        public static ErrorDetail NotFound() => ErrorDetail.FromCode("PEE_03");

        public static ErrorDetail AlreadyExist() => ErrorDetail.FromCode("PEE_04");
    }

    public static class DistrictError
    {
        public static ErrorDetail CreateFailed() => ErrorDetail.FromCode("DIE_01");

        public static ErrorDetail UpdateFailed() => ErrorDetail.FromCode("DIE_02");

        public static ErrorDetail NotFound() => ErrorDetail.FromCode("DIE_03");
    }

    public static class SeedServerError
    {
        public static ErrorDetail CreateFailed() => ErrorDetail.FromCode("SSE_01");

        public static ErrorDetail UpdateFailed() => ErrorDetail.FromCode("SSE_02");

        public static ErrorDetail NotFound() => ErrorDetail.FromCode("SSE_03");

        public static ErrorDetail AlreadyExist() => ErrorDetail.FromCode("SSE_04");

        public static ErrorDetail ConnectionFailed() => ErrorDetail.FromCode("SSE_05");

    }

    public static class SeedFunctionError
    {
        public static ErrorDetail CreateFailed() => ErrorDetail.FromCode("SFE_01");

        public static ErrorDetail UpdateFailed() => ErrorDetail.FromCode("SFE_02");

        public static ErrorDetail NotFound() => ErrorDetail.FromCode("SFE_03");

        public static ErrorDetail AlreadyExist() => ErrorDetail.FromCode("SFE_04");

    }

    public static class SeedTemplateError
    {
        public static ErrorDetail CreateFailed() => ErrorDetail.FromCode("STE_01");

        public static ErrorDetail UpdateFailed() => ErrorDetail.FromCode("STE_02");

        public static ErrorDetail NotFound() => ErrorDetail.FromCode("STE_03");

        public static ErrorDetail AlreadyExist() => ErrorDetail.FromCode("STE_04");

    }

    public static class SeedExecutionError
    {
        public static ErrorDetail CustomError() => ErrorDetail.FromCode("SEE_01");
    }
}
