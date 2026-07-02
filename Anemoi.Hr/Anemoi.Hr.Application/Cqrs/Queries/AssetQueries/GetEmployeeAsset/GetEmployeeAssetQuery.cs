using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Queries.AssetQueries.GetEmployeeAsset;

public sealed record GetEmployeeAssetQuery(
    EmployeeAssetId Id) : IQueryOne<EmployeeAssetResponse>;
