using Anemoi.BuildingBlock.Application.Responses;

namespace Anemoi.Hr.Application.Configurations;

public static class HrErrorResponses
{
    public static ErrorDetailResponse Create(string code)
    {
        return new ErrorDetailResponse
        {
            Code = code,
            Messages = [code]
        };
    }
}
