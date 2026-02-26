using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Contract.Identity.Responses;
using Anemoi.Grpc.Identity;
using Riok.Mapperly.Abstractions;
using Anemoi.BuildingBlock.Application.Errors;

namespace Anemoi.Centralize.Application.Mappings;

[Mapper]
public partial class CentralizeMapper
{
    // Identity Mappings
    public AuthenticationSuccessResponse ToAuthenticationSuccessResponse(AuthenticateSucceed authenticateSucceed)
    {
        var response = MapToAuthenticationSuccessResponse(authenticateSucceed);
        response.ExpiredIn = authenticateSucceed.ExpiredIn.ToDateTime();
        return response;
    }

    [MapperIgnoreTarget(nameof(AuthenticationSuccessResponse.ExpiredIn))]
    private partial AuthenticationSuccessResponse MapToAuthenticationSuccessResponse(AuthenticateSucceed authenticateSucceed);

    // S3 Mappings
    public ErrorDetailResponse ToErrorDetailResponse(Amazon.S3.AmazonS3Exception exception)
    {
        return new ErrorDetailResponse
        {
            Code = exception.ErrorCode,
            Messages = [exception.Message]
        };
    }

    // ErrorDetail Mappings
    public partial ErrorDetailResponse ToErrorDetailResponse(ErrorDetail errorDetail);
    public partial ErrorDetail ToErrorDetail(ErrorDetailResponse errorDetailResponse);
    public partial ErrorDetailResponse ToErrorDetailResponse(ErrorDetailResult errorDetailResult);
}
