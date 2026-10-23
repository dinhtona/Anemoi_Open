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

}
