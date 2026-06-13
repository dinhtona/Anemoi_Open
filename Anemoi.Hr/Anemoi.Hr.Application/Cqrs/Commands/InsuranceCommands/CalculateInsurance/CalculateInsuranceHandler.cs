using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Insurance;
using Anemoi.Hr.ModelIds.ModelIds;
using OneOf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Commands.InsuranceCommands.CalculateInsurance;

public sealed class CalculateInsuranceHandler(
    ISqlRepository<InsuranceRuleSet> ruleSetRepository,
    ISqlRepository<InsuranceContributionRule> contributionRuleRepository,
    ISqlRepository<InsuranceCalculationSnapshot> snapshotRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CalculateInsuranceCommand, OneOf<CalculateInsuranceResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<CalculateInsuranceResponse, ErrorDetailResponse>> Handle(
        CalculateInsuranceCommand request,
        CancellationToken cancellationToken)
    {
        var countryCode = request.CountryCode.Trim().ToUpperInvariant();
        var insuranceType = request.InsuranceType.Trim().ToUpperInvariant();

        if (request.PeriodStart > request.PeriodEnd)
        {
            return HrErrorResponses.Create("HR_INSURANCE_CALCULATION_INVALID_PERIOD");
        }

        if (request.GrossSalarySnapshot < 0)
        {
            return HrErrorResponses.Create("HR_INSURANCE_CALCULATION_NEGATIVE_SALARY");
        }

        // Find active rule set for the period
        var activeRuleSets = await ruleSetRepository.GetManyByConditionAsync(
            x => x.CountryCode == countryCode && x.InsuranceType == insuranceType && x.Status == InsuranceRuleSetStatuses.Active &&
                 x.EffectiveFrom <= request.PeriodEnd &&
                 (x.EffectiveTo == null || x.EffectiveTo >= request.PeriodStart),
            token: cancellationToken);

        var ruleSet = activeRuleSets.FirstOrDefault();
        if (ruleSet is null)
        {
            return HrErrorResponses.Create("HR_INSURANCE_RULE_SET_NOT_FOUND");
        }

        // Load contribution rules
        var contributionRules = await contributionRuleRepository.GetManyByConditionAsync(
            x => x.RuleSetId == ruleSet.Id,
            token: cancellationToken);

        var sortedRules = contributionRules.OrderBy(x => x.SortOrder).ToList();

        // Calculate contributions per rule
        var items = new List<InsuranceCalculationSnapshotItem>();
        decimal totalEmployee = 0;
        decimal totalEmployer = 0;

        foreach (var rule in sortedRules)
        {
            // Determine contribution base
            decimal contributionBase = rule.SalaryBasis.Equals("ContractSalary", StringComparison.OrdinalIgnoreCase) && request.ContractSalarySnapshot.HasValue
                ? request.ContractSalarySnapshot.Value
                : request.GrossSalarySnapshot;

            // Apply ceiling and minimum clamps
            var minAmount = rule.MinimumAmount ?? 0m;
            var maxAmount = rule.CeilingAmount ?? decimal.MaxValue;
            contributionBase = Math.Clamp(contributionBase, minAmount, maxAmount);

            // Calculate contributions
            var employeeAmount = Math.Round(contributionBase * rule.EmployeeRate, 2, MidpointRounding.AwayFromZero);
            var employerAmount = Math.Round(contributionBase * rule.EmployerRate, 2, MidpointRounding.AwayFromZero);
            var totalAmount = employeeAmount + employerAmount;

            var item = new InsuranceCalculationSnapshotItem
            {
                Id = new InsuranceCalculationSnapshotItemId(IdGenerator.NextGuid()),
                InsuranceType = insuranceType,
                ContributionType = rule.ContributionType,
                ContributionBase = contributionBase,
                EmployeeRate = rule.EmployeeRate,
                EmployerRate = rule.EmployerRate,
                EmployeeAmount = employeeAmount,
                EmployerAmount = employerAmount,
                TotalAmount = totalAmount,
                SortOrder = rule.SortOrder
            };

            items.Add(item);
            totalEmployee += employeeAmount;
            totalEmployer += employerAmount;
        }

        totalEmployee = Math.Round(totalEmployee, 2, MidpointRounding.AwayFromZero);
        totalEmployer = Math.Round(totalEmployer, 2, MidpointRounding.AwayFromZero);
        var totalContribution = totalEmployee + totalEmployer;

        // Parse employee ID if provided
        EmployeeId employeeId = null;
        if (!string.IsNullOrWhiteSpace(request.EmployeeId) && Guid.TryParse(request.EmployeeId, out var empGuid))
        {
            employeeId = new EmployeeId(empGuid);
        }

        // Create snapshot
        var snapshot = new InsuranceCalculationSnapshot
        {
            Id = new InsuranceCalculationSnapshotId(IdGenerator.NextGuid()),
            EmployeeId = employeeId,
            CountryCode = countryCode,
            InsuranceType = insuranceType,
            RuleSetId = ruleSet.Id,
            RuleSetVersion = ruleSet.Version,
            Currency = request.Currency?.Trim().ToUpperInvariant() ?? "VND",
            CalculationPeriodStart = request.PeriodStart,
            CalculationPeriodEnd = request.PeriodEnd,
            InsurableSalarySnapshot = request.GrossSalarySnapshot,
            TotalEmployeeContribution = totalEmployee,
            TotalEmployerContribution = totalEmployer,
            TotalContribution = totalContribution,
            RuleSetSnapshotJson = JsonSerializer.Serialize(new
            {
                ruleSet.Id,
                ruleSet.CountryCode,
                ruleSet.InsuranceType,
                ruleSet.Name,
                ruleSet.EffectiveFrom,
                ruleSet.EffectiveTo,
                ruleSet.Status,
                ruleSet.Version
            }),
            CalculationResultJson = JsonSerializer.Serialize(items.Select(x => new
            {
                x.ContributionType,
                x.ContributionBase,
                x.EmployeeRate,
                x.EmployerRate,
                x.EmployeeAmount,
                x.EmployerAmount,
                x.TotalAmount,
                x.SortOrder
            })),
            CalculatedAt = DateTime.UtcNow,
            CalculatedBy = request.CalculatedBy ?? "system",
            SourceModule = request.SourceModule?.Trim(),
            SourceReferenceId = request.SourceReferenceId,
            Items = items
        };

        await snapshotRepository.CreateOneAsync(snapshot, cancellationToken);
        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);

        if (saveResult.IsT1)
            return HrErrorResponses.Create("HR_SAVE_CHANGES_FAILED");

        return new CalculateInsuranceResponse
        {
            SnapshotId = snapshot.Id.Value.ToString(),
            TotalEmployeeContribution = totalEmployee,
            TotalEmployerContribution = totalEmployer,
            TotalContribution = totalContribution,
            CalculationResultJson = snapshot.CalculationResultJson
        };
    }
}
