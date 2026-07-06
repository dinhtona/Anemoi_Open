using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;
using System;
using System.Collections.Generic;

namespace Anemoi.Hr.Domain.Taxation;

public sealed class TaxDeductionRule : ValueObject
{
    public TaxDeductionRuleId Id { get; set; }
    public TaxRuleSetId TaxRuleSetId { get; set; }
    public string DeductionType { get; set; }
    public decimal Amount { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public TaxRuleSet TaxRuleSet { get; set; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
    }
}
