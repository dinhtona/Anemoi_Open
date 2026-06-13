using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Taxation;
using Anemoi.Hr.ModelIds.ModelIds;
using OneOf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Commands.TaxationCommands.CalculateTax;

public sealed class CalculateTaxHandler(
    ISqlRepository<TaxRuleSet> ruleSetRepository,
    ISqlRepository<TaxBracket> bracketRepository,
    ISqlRepository<TaxDeductionRule> deductionRuleRepository,
    ISqlRepository<TaxCalculationSnapshot> snapshotRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CalculateTaxCommand, OneOf<CalculateTaxResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<CalculateTaxResponse, ErrorDetailResponse>> Handle(
        CalculateTaxCommand request,
        CancellationToken cancellationToken)
    {
        var countryCode = request.CountryCode.Trim().ToUpperInvariant();
        var taxType = request.TaxType.Trim().ToUpperInvariant();

        if (request.PeriodStart > request.PeriodEnd)
        {
            return HrErrorResponses.Create("HR_TAX_CALCULATION_INVALID_PERIOD");
        }

        if (request.GrossIncome < 0 || request.TaxableIncome < 0)
        {
            return HrErrorResponses.Create("HR_TAX_CALCULATION_NEGATIVE_INCOME");
        }

        // Find active rule set for the period
        var activeRuleSets = await ruleSetRepository.GetManyByConditionAsync(
            x => x.CountryCode == countryCode && x.TaxType == taxType && x.Status == "Active" &&
                 x.EffectiveFrom <= request.PeriodEnd &&
                 (x.EffectiveTo == null || x.EffectiveTo >= request.PeriodStart),
            token: cancellationToken);

        var ruleSet = activeRuleSets.FirstOrDefault();
        if (ruleSet is null)
        {
            return HrErrorResponses.Create("HR_TAX_RULE_SET_NOT_FOUND");
        }

        // Load brackets and deduction rules
        var brackets = await bracketRepository.GetManyByConditionAsync(
            x => x.TaxRuleSetId == ruleSet.Id,
            token: cancellationToken);

        var deductionRules = await deductionRuleRepository.GetManyByConditionAsync(
            x => x.TaxRuleSetId == ruleSet.Id && x.IsActive,
            token: cancellationToken);

        // Sort brackets by sort order
        var sortedBrackets = brackets.OrderBy(x => x.SortOrder).ToList();

        // 1. Calculate Deductions
        var deductionItems = new List<DeductionSnapshotItem>();
        decimal totalDeductions = 0;

        foreach (var rule in deductionRules)
        {
            decimal ruleAmount = rule.Amount;
            decimal multiplier = 1;

            if (rule.DeductionType.Equals("PersonalDeduction", StringComparison.OrdinalIgnoreCase))
            {
                multiplier = 1;
            }
            else if (rule.DeductionType.Equals("DependentDeduction", StringComparison.OrdinalIgnoreCase))
            {
                decimal dependents = 0;
                if (request.DeductionInputs != null &&
                    (request.DeductionInputs.TryGetValue("Dependents", out var depVal) ||
                     request.DeductionInputs.TryGetValue("DependentCount", out depVal)))
                {
                    dependents = depVal;
                }
                multiplier = Math.Max(0, dependents);
            }
            else
            {
                // Custom deduction type, lookup in inputs if present
                if (request.DeductionInputs != null && request.DeductionInputs.TryGetValue(rule.DeductionType, out var val))
                {
                    multiplier = Math.Max(0, val);
                }
                else
                {
                    multiplier = 0; // Not applied if not in inputs
                }
            }

            var itemAmount = ruleAmount * multiplier;
            if (itemAmount > 0)
            {
                deductionItems.Add(new DeductionSnapshotItem
                {
                    Type = rule.DeductionType,
                    RateOrAmount = ruleAmount,
                    Multiplier = multiplier,
                    TotalAmount = itemAmount
                });
                totalDeductions += itemAmount;
            }
        }

        // Add additional ad-hoc deductions from inputs not matching registered rules
        if (request.DeductionInputs != null)
        {
            foreach (var input in request.DeductionInputs)
            {
                if (input.Key.Equals("Dependents", StringComparison.OrdinalIgnoreCase) ||
                    input.Key.Equals("DependentCount", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var ruleMatched = deductionRules.Any(r => r.DeductionType.Equals(input.Key, StringComparison.OrdinalIgnoreCase));
                if (!ruleMatched && input.Value > 0)
                {
                    deductionItems.Add(new DeductionSnapshotItem
                    {
                        Type = input.Key,
                        RateOrAmount = input.Value,
                        Multiplier = 1,
                        TotalAmount = input.Value
                    });
                    totalDeductions += input.Value;
                }
            }
        }

        // 2. Determine Taxable Income
        decimal taxableIncome = request.TaxableIncome;
        if (taxableIncome == 0 && request.GrossIncome > 0)
        {
            taxableIncome = Math.Max(0, request.GrossIncome - totalDeductions);
        }

        // 3. Progressive Tax Calculation
        var steps = new List<CalculationStepDetail>();
        decimal remainingIncome = taxableIncome;
        decimal totalTax = 0;

        foreach (var bracket in sortedBrackets)
        {
            if (remainingIncome <= 0)
                break;

            decimal rangeLimit = (bracket.ToAmount ?? decimal.MaxValue) - bracket.FromAmount;
            decimal taxablePortion = Math.Min(remainingIncome, rangeLimit);
            decimal bracketTax = taxablePortion * bracket.Rate;

            steps.Add(new CalculationStepDetail
            {
                SortOrder = bracket.SortOrder,
                FromAmount = bracket.FromAmount,
                ToAmount = bracket.ToAmount,
                Rate = bracket.Rate,
                TaxablePortion = taxablePortion,
                TaxAmount = bracketTax
            });

            totalTax += bracketTax;
            remainingIncome -= taxablePortion;
        }

        // Round total tax to 2 decimal places
        totalTax = Math.Round(totalTax, 2, MidpointRounding.AwayFromZero);

        // Parse employee ID and payroll run ID if provided
        EmployeeId employeeId = null;
        if (!string.IsNullOrWhiteSpace(request.EmployeeId) && Guid.TryParse(request.EmployeeId, out var empGuid))
        {
            employeeId = new EmployeeId(empGuid);
        }

        PayrollRunId payrollRunId = null;
        if (request.SourceModule.Equals("Payroll", StringComparison.OrdinalIgnoreCase) && request.SourceReferenceId.HasValue)
        {
            payrollRunId = new PayrollRunId(request.SourceReferenceId.Value);
        }

        // 4. Create Snapshot
        var snapshot = new TaxCalculationSnapshot
        {
            Id = new TaxCalculationSnapshotId(IdGenerator.NextGuid()),
            EmployeeId = employeeId,
            PayrollRunId = payrollRunId,
            CountryCode = countryCode,
            TaxType = taxType,
            TaxRuleSetId = ruleSet.Id,
            TaxRuleSetVersion = ruleSet.Version,
            GrossIncomeSnapshot = request.GrossIncome,
            TaxableIncomeSnapshot = taxableIncome,
            RuleSetSnapshotJson = JsonSerializer.Serialize(new
            {
                ruleSet.Id,
                ruleSet.CountryCode,
                ruleSet.TaxType,
                ruleSet.Name,
                ruleSet.EffectiveFrom,
                ruleSet.EffectiveTo,
                ruleSet.Status,
                ruleSet.Version
            }),
            DeductionSnapshotJson = JsonSerializer.Serialize(deductionItems),
            BracketSnapshotJson = JsonSerializer.Serialize(sortedBrackets.Select(x => new
            {
                x.Id,
                x.FromAmount,
                x.ToAmount,
                x.Rate,
                x.QuickDeductionAmount,
                x.SortOrder
            })),
            CalculationResultJson = JsonSerializer.Serialize(steps),
            TotalTaxAmount = totalTax,
            Currency = request.Currency.Trim().ToUpperInvariant(),
            CalculationPeriodStart = request.PeriodStart,
            CalculationPeriodEnd = request.PeriodEnd,
            CalculatedAt = DateTime.UtcNow,
            CalculatedBy = request.CalculatedBy ?? "system",
            SourceModule = request.SourceModule.Trim(),
            SourceReferenceId = request.SourceReferenceId
        };

        await snapshotRepository.CreateOneAsync(snapshot, cancellationToken);
        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);

        if (saveResult.IsT1)
            return HrErrorResponses.Create("HR_SAVE_CHANGES_FAILED");

        return new CalculateTaxResponse
        {
            SnapshotId = snapshot.Id.Value.ToString(),
            TotalTaxAmount = totalTax,
            TaxableIncome = taxableIncome,
            CalculationResultJson = snapshot.CalculationResultJson
        };
    }

    private sealed class DeductionSnapshotItem
    {
        public string Type { get; set; }
        public decimal RateOrAmount { get; set; }
        public decimal Multiplier { get; set; }
        public decimal TotalAmount { get; set; }
    }

    private sealed class CalculationStepDetail
    {
        public int SortOrder { get; set; }
        public decimal FromAmount { get; set; }
        public decimal? ToAmount { get; set; }
        public decimal Rate { get; set; }
        public decimal TaxablePortion { get; set; }
        public decimal TaxAmount { get; set; }
    }
}
