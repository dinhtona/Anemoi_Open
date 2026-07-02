using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.EmployeeAssets;

namespace Anemoi.Hr.Application.Mappings;

public sealed class EmployeeAssetMapper
{
    public EmployeeAssetResponse ToResponse(EmployeeAsset asset)
    {
        if (asset is null) return null;
        return new EmployeeAssetResponse
        {
            Id = asset.Id.Value.ToString(),
            EmployeeId = asset.EmployeeId?.Value.ToString(),
            AssetType = asset.AssetType.Value,
            AssetTag = asset.AssetTag,
            Name = asset.Name,
            Brand = asset.Brand,
            Model = asset.Model,
            SerialNumber = asset.SerialNumber,
            AssetStatus = asset.AssetStatus.Value,
            AssignedDate = asset.AssignedDate,
            ReturnedDate = asset.ReturnedDate,
            Notes = asset.Notes,
            IsArchived = asset.IsArchived,
            CreatedAt = asset.CreatedAt,
            UpdatedAt = asset.UpdatedAt
        };
    }

    public IReadOnlyCollection<EmployeeAssetResponse> ToResponses(IEnumerable<EmployeeAsset> assets)
    {
        if (assets is null) return [];
        return assets.Select(ToResponse).ToList();
    }
}
