using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;
using System;
using System.Collections.Generic;

namespace Anemoi.Hr.Domain.Taxation;

public sealed class TaxCalculationSnapshot : ValueObject
{
    public TaxCalculationSnapshotId Id { get; set; }
    public EmployeeId? EmployeeId { get; set; }
    public PayrollRunId? PayrollRunId { get; set; }
    public string CountryCode { get; set; }
    public string TaxType { get; set; }
    public TaxRuleSetId TaxRuleSetId { get; set; }
    public int TaxRuleSetVersion { get; set; }
    public decimal GrossIncomeSnapshot { get; set; }
    public decimal TaxableIncomeSnapshot { get; set; }
    public string RuleSetSnapshotJson { get; set; }
    public string DeductionSnapshotJson { get; set; }
    public string BracketSnapshotJson { get; set; }
    public string CalculationResultJson { get; set; }
    public decimal TotalTaxAmount { get; set; }
    public string Currency { get; set; }
    public DateOnly CalculationPeriodStart { get; set; }
    public DateOnly CalculationPeriodEnd { get; set; }
    public DateTime CalculatedAt { get; set; }
    public string CalculatedBy { get; set; }
    public string SourceModule { get; set; }
    public Guid? SourceReferenceId { get; set; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
    }
}
