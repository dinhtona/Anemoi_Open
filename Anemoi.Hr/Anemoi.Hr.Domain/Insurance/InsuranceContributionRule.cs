using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;
using System;

namespace Anemoi.Hr.Domain.Insurance;

public sealed class InsuranceContributionRule : Entity<InsuranceContributionRuleId>
{
    public InsuranceRuleSetId RuleSetId { get; set; }
    public string ContributionType { get; set; }
    public decimal EmployeeRate { get; set; }
    public decimal EmployerRate { get; set; }
    public decimal? CeilingAmount { get; set; }
    public decimal? MinimumAmount { get; set; }
    public string SalaryBasis { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public InsuranceRuleSet RuleSet { get; set; }
}
