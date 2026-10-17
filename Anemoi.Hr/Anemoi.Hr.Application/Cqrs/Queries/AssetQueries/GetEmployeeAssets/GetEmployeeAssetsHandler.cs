using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.EmployeeAssets;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Queries.AssetQueries.GetEmployeeAssets;

public sealed class GetEmployeeAssetsHandler(
    ISqlRepository<EmployeeAsset> assetRepository,
    EmployeeAssetMapper mapper)
    : IQueryHandler<GetEmployeeAssetsQuery, OneOf<IReadOnlyCollection<EmployeeAssetResponse>, ErrorDetailResponse>>
{
    public async Task<OneOf<IReadOnlyCollection<EmployeeAssetResponse>, ErrorDetailResponse>> Handle(
        GetEmployeeAssetsQuery request, CancellationToken cancellationToken)
    {
        var assets = await assetRepository.GetQueryable()
            .Where(x => x.EmployeeId == request.EmployeeId && !x.IsArchived)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return OneOf<IReadOnlyCollection<EmployeeAssetResponse>, ErrorDetailResponse>.FromT0(mapper.ToResponses(assets));
    }
}
