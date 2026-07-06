using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.Domain.Positions;
using Anemoi.Hr.ModelIds.ModelIds;
using System;
using System.Collections.Generic;

namespace Anemoi.Hr.Domain.Compensation;

public sealed class PositionAllowance : ValueObject
{
    public PositionAllowanceId Id { get; set; }
    public PositionId PositionId { get; set; }
    public AllowanceTypeId AllowanceTypeId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Position Position { get; set; }
    public AllowanceType AllowanceType { get; set; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
    }
}
