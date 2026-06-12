using System;
using System.Collections.Generic;

namespace Anemoi.Hr.Application.Responses;

public sealed class TaxRuleSetResponse
{
    public string Id { get; set; }
    public string CountryCode { get; set; }
    public string TaxType { get; set; }
    public string Name { get; set; }
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public string Status { get; set; }
    public int Version { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; }
    public string UpdatedBy { get; set; }
    public List<TaxBracketResponse> Brackets { get; set; } = [];
    public List<TaxDeductionRuleResponse> DeductionRules { get; set; } = [];
}

public sealed class TaxBracketResponse
{
    public string Id { get; set; }
    public string TaxRuleSetId { get; set; }
    public decimal FromAmount { get; set; }
    public decimal? ToAmount { get; set; }
    public decimal Rate { get; set; }
    public decimal? QuickDeductionAmount { get; set; }
    public int SortOrder { get; set; }
}

public sealed class TaxDeductionRuleResponse
{
    public string Id { get; set; }
    public string TaxRuleSetId { get; set; }
    public string DeductionType { get; set; }
    public decimal Amount { get; set; }
    public bool IsActive { get; set; }
}

public sealed class TaxCalculationSnapshotResponse
{
    public string Id { get; set; }
    public string EmployeeId { get; set; }
    public string PayrollRunId { get; set; }
    public string CountryCode { get; set; }
    public string TaxType { get; set; }
    public string TaxRuleSetId { get; set; }
    public int TaxRuleSetVersion { get; set; }
    public decimal GrossIncomeSnapshot { get; set; }
    public decimal TaxableIncomeSnapshot { get; set; }
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
}

public sealed class CalculateTaxResponse
{
    public string SnapshotId { get; set; }
    public decimal TotalTaxAmount { get; set; }
    public decimal TaxableIncome { get; set; }
    public string CalculationResultJson { get; set; }
}

public sealed class CreateTaxRuleSetResponse
{
    public string TaxRuleSetId { get; set; }
}

public sealed class CreateTaxBracketResponse
{
    public string TaxBracketId { get; set; }
}

public sealed class CreateTaxDeductionRuleResponse
{
    public string TaxDeductionRuleId { get; set; }
}
