using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.EmployeeAssets;

public sealed class EmployeeAsset : Entity<EmployeeAssetId>
{
    public EmployeeId? EmployeeId { get; private set; }
    public AssetType AssetType { get; private set; }
    public string AssetTag { get; private set; }
    public string Name { get; private set; }
    public string? Brand { get; private set; }
    public string? Model { get; private set; }
    public string? SerialNumber { get; private set; }
    public AssetStatus AssetStatus { get; private set; }
    public DateTime AssignedDate { get; private set; }
    public DateTime? ReturnedDate { get; private set; }
    public string? Notes { get; private set; }
    public bool IsArchived { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private EmployeeAsset() { }

    public static EmployeeAsset Create(
        EmployeeAssetId id,
        AssetType assetType,
        string assetTag,
        string name,
        string? brand = null,
        string? model = null,
        string? serialNumber = null,
        string? notes = null)
    {
        return new EmployeeAsset
        {
            Id = id,
            EmployeeId = null,
            AssetType = assetType,
            AssetTag = assetTag,
            Name = name,
            Brand = brand,
            Model = model,
            SerialNumber = serialNumber,
            AssetStatus = AssetStatus.Available,
            AssignedDate = DateTime.UtcNow,
            Notes = notes,
            IsArchived = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Assign(EmployeeId employeeId)
    {
        if (AssetStatus != AssetStatus.Available && AssetStatus != AssetStatus.Returned)
            throw new InvalidOperationException($"Cannot assign asset in status {AssetStatus}");

        EmployeeId = employeeId;
        AssetStatus = AssetStatus.Assigned;
        AssignedDate = DateTime.UtcNow;
        ReturnedDate = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Return()
    {
        if (AssetStatus != AssetStatus.Assigned)
            throw new InvalidOperationException($"Cannot return asset in status {AssetStatus}");

        EmployeeId = null;
        AssetStatus = AssetStatus.Returned;
        ReturnedDate = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkLost()
    {
        if (AssetStatus != AssetStatus.Assigned)
            throw new InvalidOperationException($"Cannot mark asset lost in status {AssetStatus}");

        AssetStatus = AssetStatus.Lost;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkDamaged()
    {
        if (AssetStatus != AssetStatus.Assigned)
            throw new InvalidOperationException($"Cannot mark asset damaged in status {AssetStatus}");

        AssetStatus = AssetStatus.Damaged;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateInfo(
        string name,
        string? brand = null,
        string? model = null,
        string? serialNumber = null,
        string? notes = null)
    {
        Name = name;
        Brand = brand;
        Model = model;
        SerialNumber = serialNumber;
        Notes = notes;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Archive()
    {
        if (IsArchived) return;
        IsArchived = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
