using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.EmployeeAssets;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Queries.AssetQueries.GetEmployeeAsset;

public sealed class GetEmployeeAssetHandler(
    ISqlRepository<EmployeeAsset> assetRepository,
    EmployeeAssetMapper mapper)
    : IQueryHandler<GetEmployeeAssetQuery, OneOf<EmployeeAssetResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<EmployeeAssetResponse, ErrorDetailResponse>> Handle(
        GetEmployeeAssetQuery request, CancellationToken cancellationToken)
    {
        var asset = await assetRepository.GetFirstByConditionAsync(
            x => x.Id == request.Id, null, cancellationToken);

        return asset is null
            ? HrErrorResponses.Create(HrBusinessErrorCodes.EmployeeAssetNotFound)
            : mapper.ToResponse(asset);
    }
}
