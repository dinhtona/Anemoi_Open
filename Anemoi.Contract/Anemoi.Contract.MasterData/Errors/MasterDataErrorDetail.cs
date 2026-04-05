using Anemoi.BuildingBlock.Application.Errors;

namespace Anemoi.Contract.MasterData.Errors;

public static class MasterDataErrorDetail
{
    public static class ProvinceError
    {
        public static ErrorDetail CreateFailed() => new()
        {
            Messages = ["Error while creating a new province!"], Code = "PEE_01"
        };

        public static ErrorDetail UpdateFailed() => new()
        {
            Messages = ["Error while updating an exist province!"], Code = "PEE_02"
        };

        public static ErrorDetail NotFound() => new()
        {
            Messages = ["Province was not found!"], Code = "PEE_03"
        };

        public static ErrorDetail AlreadyExist() => new()
        {
            Messages = ["Province is already exist!"], Code = "PEE_04"
        };
    }

    public static class DistrictError
    {
        public static ErrorDetail CreateFailed() => new()
        {
            Messages = ["Error while creating a new district!"], Code = "DIE_01"
        };

        public static ErrorDetail UpdateFailed() => new()
        {
            Messages = ["Error while updating an exist district!"], Code = "DIE_02"
        };

        public static ErrorDetail NotFound() => new()
        {
            Messages = ["District was not found!"], Code = "DIE_03"
        };
    }

    public static class SeedServerError
    {
        public static ErrorDetail CreateFailed() => new()
        {
            Messages = ["Error while creating a new seed server!"], Code = "SSE_01"
        };

        public static ErrorDetail UpdateFailed() => new()
        {
            Messages = ["Error while updating a seed server!"], Code = "SSE_02"
        };

        public static ErrorDetail NotFound() => new()
        {
            Messages = ["Seed server was not found!"], Code = "SSE_03"
        };

        public static ErrorDetail AlreadyExist() => new()
        {
            Messages = ["Seed server name already exists!"], Code = "SSE_04"
        };

        public static ErrorDetail ConnectionFailed() => new()
        {
            Messages = ["Database connection test failed!"], Code = "SSE_05"
        };

    }

    public static class SeedFunctionError
    {
        public static ErrorDetail CreateFailed() => new()
        {
            Messages = ["Error while creating a new seed function!"], Code = "SFE_01"
        };

        public static ErrorDetail UpdateFailed() => new()
        {
            Messages = ["Error while updating a seed function!"], Code = "SFE_02"
        };

        public static ErrorDetail NotFound() => new()
        {
            Messages = ["Seed function was not found!"], Code = "SFE_03"
        };

        public static ErrorDetail AlreadyExist() => new()
        {
            Messages = ["Seed function name already exists!"], Code = "SFE_04"
        };

    }

    public static class SeedTemplateError
    {
        public static ErrorDetail CreateFailed() => new()
        {
            Messages = ["Error while creating a new seed template!"], Code = "STE_01"
        };

        public static ErrorDetail UpdateFailed() => new()
        {
            Messages = ["Error while updating a seed template!"], Code = "STE_02"
        };

        public static ErrorDetail NotFound() => new()
        {
            Messages = ["Seed template was not found!"], Code = "STE_03"
        };

        public static ErrorDetail AlreadyExist() => new()
        {
            Messages = ["Seed template name already exists!"], Code = "STE_04"
        };

    }

    public static class SeedExecutionError
    {
        public static ErrorDetail CustomError(string message) => new()
        {
            Messages = [message], Code = "SEE_01"
        };
    }
}