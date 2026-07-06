namespace Anemoi.Hr.Application.Responses;

public sealed class EmployeeAssetResponse
{
    public string Id { get; set; }
    public string EmployeeId { get; set; }
    public string AssetType { get; set; }
    public string AssetTag { get; set; }
    public string Name { get; set; }
    public string Brand { get; set; }
    public string Model { get; set; }
    public string SerialNumber { get; set; }
    public string AssetStatus { get; set; }
    public DateTime AssignedDate { get; set; }
    public DateTime? ReturnedDate { get; set; }
    public string Notes { get; set; }
    public bool IsArchived { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
