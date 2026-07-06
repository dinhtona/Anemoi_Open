using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.Insurance;

public sealed class InsuranceCalculationSnapshotItem : Entity<InsuranceCalculationSnapshotItemId>
{
    public InsuranceCalculationSnapshotId SnapshotId { get; set; }
    public string InsuranceType { get; set; }
    public string ContributionType { get; set; }
    public decimal ContributionBase { get; set; }
    public decimal EmployeeRate { get; set; }
    public decimal EmployerRate { get; set; }
    public decimal EmployeeAmount { get; set; }
    public decimal EmployerAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public int SortOrder { get; set; }

    public InsuranceCalculationSnapshot Snapshot { get; set; }
}
