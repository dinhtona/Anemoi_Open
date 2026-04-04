using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Errors;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Contract.MasterData.Errors;
using Anemoi.Contract.MasterData.Queries.SeedDataQueries.GetDbSchema;
using Anemoi.Contract.MasterData.Responses;
using Anemoi.MasterData.Application.Abstractions;
using Anemoi.MasterData.Application.Mappings;
using Anemoi.MasterData.Domain.Models;
using MediatR;
using OneOf;

namespace Anemoi.MasterData.Application.Cqrs.Queries.SeedDataQueries.GetDbSchema;

public sealed class GetDbSchemaHandler(
    ISqlRepository<SeedServer> sqlRepository,
    IDbDiscoveryService dbDiscoveryService,
    MasterDataMapper mapper)
    : IRequestHandler<GetDbSchemaQuery, OneOf<DbSchemaResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<DbSchemaResponse, ErrorDetailResponse>> Handle(GetDbSchemaQuery request, CancellationToken cancellationToken)
    {
        var server = await sqlRepository.GetFirstByConditionAsync(x => x.Id == request.SeedServerId);
        if (server is null) return mapper.ToErrorDetailResponse(MasterDataErrorDetail.SeedServerError.NotFound());

        var schema = await dbDiscoveryService.GetSchemaAsync(server.ConnectionString, cancellationToken);
        return schema;
    }
}
