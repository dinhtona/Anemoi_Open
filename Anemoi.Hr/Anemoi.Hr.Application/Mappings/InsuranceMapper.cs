using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Insurance;
using Riok.Mapperly.Abstractions;
using System.Linq;

namespace Anemoi.Hr.Application.Mappings;

[Mapper]
public partial class InsuranceMapper
{
    public InsuranceRuleSetResponse ToInsuranceRuleSetResponse(InsuranceRuleSet entity)
    {
        if (entity is null) return null;
        return new InsuranceRuleSetResponse
        {
            Id = entity.Id.Value.ToString(),
            CountryCode = entity.CountryCode,
            InsuranceType = entity.InsuranceType,
            Name = entity.Name,
            Currency = entity.Currency,
            EffectiveFrom = entity.EffectiveFrom,
            EffectiveTo = entity.EffectiveTo,
            Status = entity.Status,
            Version = entity.Version,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedBy = entity.UpdatedBy,
            ContributionRules = entity.ContributionRules?.Select(ToInsuranceContributionRuleResponse).ToList() ?? []
        };
    }

    public InsuranceContributionRuleResponse ToInsuranceContributionRuleResponse(InsuranceContributionRule entity)
    {
        if (entity is null) return null;
        return new InsuranceContributionRuleResponse
        {
            Id = entity.Id.Value.ToString(),
            RuleSetId = entity.RuleSetId.Value.ToString(),
            ContributionType = entity.ContributionType,
            EmployeeRate = entity.EmployeeRate,
            EmployerRate = entity.EmployerRate,
            CeilingAmount = entity.CeilingAmount,
            MinimumAmount = entity.MinimumAmount,
            SalaryBasis = entity.SalaryBasis,
            SortOrder = entity.SortOrder
        };
    }

    public InsuranceCalculationSnapshotResponse ToInsuranceCalculationSnapshotResponse(InsuranceCalculationSnapshot entity)
    {
        if (entity is null) return null;
        return new InsuranceCalculationSnapshotResponse
        {
            Id = entity.Id.Value.ToString(),
            EmployeeId = entity.EmployeeId?.Value.ToString(),
            CountryCode = entity.CountryCode,
            InsuranceType = entity.InsuranceType,
            RuleSetId = entity.RuleSetId.Value.ToString(),
            RuleSetVersion = entity.RuleSetVersion,
            Currency = entity.Currency,
            CalculationPeriodStart = entity.CalculationPeriodStart,
            CalculationPeriodEnd = entity.CalculationPeriodEnd,
            InsurableSalarySnapshot = entity.InsurableSalarySnapshot,
            TotalEmployeeContribution = entity.TotalEmployeeContribution,
            TotalEmployerContribution = entity.TotalEmployerContribution,
            TotalContribution = entity.TotalContribution,
            RuleSetSnapshotJson = entity.RuleSetSnapshotJson,
            CalculationResultJson = entity.CalculationResultJson,
            CalculatedAt = entity.CalculatedAt,
            CalculatedBy = entity.CalculatedBy,
            SourceModule = entity.SourceModule,
            SourceReferenceId = entity.SourceReferenceId,
            Items = entity.Items?.Select(ToInsuranceCalculationSnapshotItemResponse).ToList() ?? []
        };
    }

    public InsuranceCalculationSnapshotItemResponse ToInsuranceCalculationSnapshotItemResponse(InsuranceCalculationSnapshotItem entity)
    {
        if (entity is null) return null;
        return new InsuranceCalculationSnapshotItemResponse
        {
            Id = entity.Id.Value.ToString(),
            SnapshotId = entity.SnapshotId.Value.ToString(),
            InsuranceType = entity.InsuranceType,
            ContributionType = entity.ContributionType,
            ContributionBase = entity.ContributionBase,
            EmployeeRate = entity.EmployeeRate,
            EmployerRate = entity.EmployerRate,
            EmployeeAmount = entity.EmployeeAmount,
            EmployerAmount = entity.EmployerAmount,
            TotalAmount = entity.TotalAmount,
            SortOrder = entity.SortOrder
        };
    }
}
