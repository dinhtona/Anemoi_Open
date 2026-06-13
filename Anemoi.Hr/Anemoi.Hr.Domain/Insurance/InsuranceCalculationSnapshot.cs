using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;
using System;
using System.Collections.Generic;

namespace Anemoi.Hr.Domain.Insurance;

public sealed class InsuranceCalculationSnapshot : Entity<InsuranceCalculationSnapshotId>
{
    public EmployeeId? EmployeeId { get; set; }
    public string CountryCode { get; set; }
    public string InsuranceType { get; set; }
    public InsuranceRuleSetId RuleSetId { get; set; }
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

    public List<InsuranceCalculationSnapshotItem> Items { get; set; } = [];
}
