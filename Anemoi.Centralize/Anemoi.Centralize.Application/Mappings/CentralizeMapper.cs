using Anemoi.BuildingBlock.Application.Responses;
using Riok.Mapperly.Abstractions;
using Anemoi.BuildingBlock.Application.Errors;

namespace Anemoi.Centralize.Application.Mappings;

[Mapper]
public partial class CentralizeMapper
{
    // S3 Mappings
    public ErrorDetailResponse ToErrorDetailResponse(Amazon.S3.AmazonS3Exception exception)
    {
        return new ErrorDetailResponse
        {
            Code = "S3OperationFailed",
            Messages = ["S3OperationFailed"]
        };
    }

    // ErrorDetail Mappings
    public ErrorDetailResponse ToErrorDetailResponse(ErrorDetail errorDetail)
    {
        return new ErrorDetailResponse
        {
            Code = errorDetail.Code,
            Messages = errorDetail.Messages
        };
    }

    public ErrorDetail ToErrorDetail(ErrorDetailResponse errorDetailResponse)
    {
        return new ErrorDetail
        {
            Code = errorDetailResponse.Code,
            Messages = errorDetailResponse.Messages
        };
    }
}
