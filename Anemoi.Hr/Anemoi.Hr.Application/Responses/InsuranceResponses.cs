using System;
using System.Collections.Generic;

namespace Anemoi.Hr.Application.Responses;

public sealed class InsuranceRuleSetResponse
{
    public string Id { get; set; }
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
    public List<InsuranceContributionRuleResponse> ContributionRules { get; set; } = [];
}

public sealed class InsuranceContributionRuleResponse
{
    public string Id { get; set; }
    public string RuleSetId { get; set; }
    public string ContributionType { get; set; }
    public decimal EmployeeRate { get; set; }
    public decimal EmployerRate { get; set; }
    public decimal? CeilingAmount { get; set; }
    public decimal? MinimumAmount { get; set; }
    public string SalaryBasis { get; set; }
    public int SortOrder { get; set; }
}

public sealed class InsuranceCalculationSnapshotResponse
{
    public string Id { get; set; }
    public string EmployeeId { get; set; }
    public string CountryCode { get; set; }
    public string InsuranceType { get; set; }
    public string RuleSetId { get; set; }
    public int RuleSetVersion { get; set; }
    public string Currency { get; set; }
    public DateOnly CalculationPeriodStart { get; set; }
    public DateOnly CalculationPeriodEnd { get; set; }
    public decimal InsurableSalarySnapshot { get; set; }
    public decimal TotalEmployeeContribution { get; set; }
    public decimal TotalEmployerContribution { get; set; }
    public decimal TotalContribution { get; set; }
    public string RuleSetSnapshotJson { get; set; }
    public string CalculationResultJson { get; set; }
    public DateTime CalculatedAt { get; set; }
    public string CalculatedBy { get; set; }
    public string SourceModule { get; set; }
    public string SourceReferenceId { get; set; }
    public List<InsuranceCalculationSnapshotItemResponse> Items { get; set; } = [];
}

public sealed class InsuranceCalculationSnapshotItemResponse
{
    public string Id { get; set; }
    public string SnapshotId { get; set; }
    public string InsuranceType { get; set; }
    public string ContributionType { get; set; }
    public decimal ContributionBase { get; set; }
    public decimal EmployeeRate { get; set; }
    public decimal EmployerRate { get; set; }
    public decimal EmployeeAmount { get; set; }
    public decimal EmployerAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public int SortOrder { get; set; }
}

public sealed class CalculateInsuranceResponse
{
    public string SnapshotId { get; set; }
    public decimal TotalEmployeeContribution { get; set; }
    public decimal TotalEmployerContribution { get; set; }
    public decimal TotalContribution { get; set; }
    public string CalculationResultJson { get; set; }
}

public sealed class CreateInsuranceRuleSetResponse
{
    public string InsuranceRuleSetId { get; set; }
}

public sealed class CreateInsuranceContributionRuleResponse
{
    public string InsuranceContributionRuleId { get; set; }
}
