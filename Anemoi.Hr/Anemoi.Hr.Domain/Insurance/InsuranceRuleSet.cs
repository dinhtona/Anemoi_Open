using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;
using System;
using System.Collections.Generic;

namespace Anemoi.Hr.Domain.Insurance;

public sealed class InsuranceRuleSet : Entity<InsuranceRuleSetId>
{
    public string CountryCode { get; set; }
    public string InsuranceType { get; set; }
    public string Name { get; set; }
    public string Currency { get; set; }
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public string Status { get; set; }
    public int Version { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; }
    public string UpdatedBy { get; set; }

    public List<InsuranceContributionRule> ContributionRules { get; set; } = [];
}
