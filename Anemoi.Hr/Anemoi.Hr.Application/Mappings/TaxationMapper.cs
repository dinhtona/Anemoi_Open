using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Taxation;
using Riok.Mapperly.Abstractions;
using System.Linq;

namespace Anemoi.Hr.Application.Mappings;

[Mapper]
public partial class TaxationMapper
{
    public TaxRuleSetResponse ToTaxRuleSetResponse(TaxRuleSet entity)
    {
        if (entity is null) return null;
        return new TaxRuleSetResponse
        {
            Id = entity.Id.Value.ToString(),
            CountryCode = entity.CountryCode,
            TaxType = entity.TaxType,
            Name = entity.Name,
            EffectiveFrom = entity.EffectiveFrom,
            EffectiveTo = entity.EffectiveTo,
            Status = entity.Status,
            Version = entity.Version,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedBy = entity.UpdatedBy,
            Brackets = entity.Brackets?.Select(ToTaxBracketResponse).ToList() ?? [],
            DeductionRules = entity.DeductionRules?.Select(ToTaxDeductionRuleResponse).ToList() ?? []
        };
    }

    public TaxBracketResponse ToTaxBracketResponse(TaxBracket entity)
    {
        if (entity is null) return null;
        return new TaxBracketResponse
        {
            Id = entity.Id.Value.ToString(),
            TaxRuleSetId = entity.TaxRuleSetId.Value.ToString(),
            FromAmount = entity.FromAmount,
            ToAmount = entity.ToAmount,
            Rate = entity.Rate,
            QuickDeductionAmount = entity.QuickDeductionAmount,
            SortOrder = entity.SortOrder
        };
    }

    public TaxDeductionRuleResponse ToTaxDeductionRuleResponse(TaxDeductionRule entity)
    {
        if (entity is null) return null;
        return new TaxDeductionRuleResponse
        {
            Id = entity.Id.Value.ToString(),
            TaxRuleSetId = entity.TaxRuleSetId.Value.ToString(),
            DeductionType = entity.DeductionType,
            Amount = entity.Amount,
            IsActive = entity.IsActive
        };
    }

    public TaxCalculationSnapshotResponse ToTaxCalculationSnapshotResponse(TaxCalculationSnapshot entity)
    {
        if (entity is null) return null;
        return new TaxCalculationSnapshotResponse
        {
            Id = entity.Id.Value.ToString(),
            EmployeeId = entity.EmployeeId?.Value.ToString(),
            PayrollRunId = entity.PayrollRunId?.Value.ToString(),
            CountryCode = entity.CountryCode,
            TaxType = entity.TaxType,
            TaxRuleSetId = entity.TaxRuleSetId.Value.ToString(),
            TaxRuleSetVersion = entity.TaxRuleSetVersion,
            GrossIncomeSnapshot = entity.GrossIncomeSnapshot,
            TaxableIncomeSnapshot = entity.TaxableIncomeSnapshot,
            RuleSetSnapshotJson = entity.RuleSetSnapshotJson,
            DeductionSnapshotJson = entity.DeductionSnapshotJson,
            BracketSnapshotJson = entity.BracketSnapshotJson,
            CalculationResultJson = entity.CalculationResultJson,
            TotalTaxAmount = entity.TotalTaxAmount,
            Currency = entity.Currency,
            CalculationPeriodStart = entity.CalculationPeriodStart,
            CalculationPeriodEnd = entity.CalculationPeriodEnd,
            CalculatedAt = entity.CalculatedAt,
            CalculatedBy = entity.CalculatedBy,
            SourceModule = entity.SourceModule,
            SourceReferenceId = entity.SourceReferenceId
        };
    }
}
