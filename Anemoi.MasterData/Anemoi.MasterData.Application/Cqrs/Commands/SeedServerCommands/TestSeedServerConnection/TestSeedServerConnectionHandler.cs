using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Application.Results;
using Anemoi.Contract.MasterData.Commands.SeedServerCommands.TestSeedServerConnection;
using Anemoi.Contract.MasterData.Errors;
using Anemoi.MasterData.Application.Abstractions;
using Anemoi.MasterData.Application.Mappings;
using MediatR;
using OneOf;
using Serilog;

namespace Anemoi.MasterData.Application.Cqrs.Commands.SeedServerCommands.TestSeedServerConnection;

public sealed class TestSeedServerConnectionHandler(
    IDbDiscoveryService dbDiscoveryService,
    MasterDataMapper mapper,
    ILogger logger)
    : IRequestHandler<TestSeedServerConnectionCommand, OneOf<None, ErrorDetailResponse>>
{
    public async Task<OneOf<None, ErrorDetailResponse>> Handle(TestSeedServerConnectionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var isSuccess = await dbDiscoveryService.TestConnectionAsync(request.ConnectionString, cancellationToken);
            if (!isSuccess)
            {
                return mapper.ToErrorDetailResponse(MasterDataErrorDetail.SeedServerError.ConnectionFailed());
            }
            return None.Value;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error while testing seed server connection");
            return mapper.ToErrorDetailResponse(MasterDataErrorDetail.SeedServerError.ConnectionFailed());
        }
    }
}
