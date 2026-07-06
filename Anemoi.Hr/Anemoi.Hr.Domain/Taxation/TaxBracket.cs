using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;
using System;
using System.Collections.Generic;

namespace Anemoi.Hr.Domain.Taxation;

public sealed class TaxBracket : ValueObject
{
    public TaxBracketId Id { get; set; }
    public TaxRuleSetId TaxRuleSetId { get; set; }
    public decimal FromAmount { get; set; }
    public decimal? ToAmount { get; set; }
    public decimal Rate { get; set; }
    public decimal? QuickDeductionAmount { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public TaxRuleSet TaxRuleSet { get; set; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
    }
}
