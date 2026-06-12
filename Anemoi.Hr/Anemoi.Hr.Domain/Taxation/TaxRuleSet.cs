using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;
using System;
using System.Collections.Generic;

namespace Anemoi.Hr.Domain.Taxation;

public sealed class TaxRuleSet : ValueObject
{
    public TaxRuleSetId Id { get; set; }
    public string CountryCode { get; set; }
    public string TaxType { get; set; }
    public string Name { get; set; }
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public string Status { get; set; } // Draft, Active, Inactive
    public int Version { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; }
    public string UpdatedBy { get; set; }

    public List<TaxBracket> Brackets { get; set; } = [];
    public List<TaxDeductionRule> DeductionRules { get; set; } = [];

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
    }
}
