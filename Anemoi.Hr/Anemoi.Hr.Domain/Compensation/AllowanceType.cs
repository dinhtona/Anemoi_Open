using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;
using System;
using System.Collections.Generic;

namespace Anemoi.Hr.Domain.Compensation;

public sealed class AllowanceType : ValueObject
{
    public AllowanceTypeId Id { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsTaxable { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
    }

    public void UpdateDetails(string name, string description, bool isTaxable, bool isActive)
    {
        Name = name?.Trim();
        Description = description?.Trim();
        IsTaxable = isTaxable;
        IsActive = isActive;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
