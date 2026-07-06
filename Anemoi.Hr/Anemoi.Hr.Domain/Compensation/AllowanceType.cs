using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.Compensation;

public sealed class AllowanceType : Entity<AllowanceTypeId>
{
    public string Code { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public bool Taxable { get; private set; }
    public bool SubjectToInsurance { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private AllowanceType() { }

    public static AllowanceType Create(
        AllowanceTypeId id,
        string code,
        string name,
        string description,
        bool taxable,
        bool subjectToInsurance,
        bool isActive = true)
    {
        return new AllowanceType
        {
            Id = id,
            Code = code,
            Name = name,
            Description = description ?? string.Empty,
            Taxable = taxable,
            SubjectToInsurance = subjectToInsurance,
            IsActive = isActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void UpdateDetails(string name, string description, bool taxable, bool subjectToInsurance, bool isActive)
    {
        Name = name?.Trim();
        Description = description?.Trim() ?? string.Empty;
        Taxable = taxable;
        SubjectToInsurance = subjectToInsurance;
        IsActive = isActive;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
