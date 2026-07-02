using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Queries.AssetQueries.GetEmployeeAssets;

public sealed record GetEmployeeAssetsQuery(
    EmployeeId EmployeeId) : IQuery<IReadOnlyCollection<EmployeeAssetResponse>>;
