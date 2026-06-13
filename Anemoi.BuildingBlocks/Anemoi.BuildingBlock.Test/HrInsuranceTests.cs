using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Results;
using Anemoi.BuildingBlock.Domain.Models;
using Anemoi.Hr.Application.Cqrs.Commands.InsuranceCommands.ActivateInsuranceRuleSet;
using Anemoi.Hr.Application.Cqrs.Commands.InsuranceCommands.CalculateInsurance;
using Anemoi.Hr.Application.Cqrs.Commands.InsuranceCommands.CreateInsuranceContributionRule;
using Anemoi.Hr.Application.Cqrs.Commands.InsuranceCommands.CreateInsuranceRuleSet;
using Anemoi.Hr.Application.Cqrs.Commands.InsuranceCommands.DeactivateInsuranceRuleSet;
using Anemoi.Hr.Application.Cqrs.Commands.InsuranceCommands.DeleteInsuranceContributionRule;
using Anemoi.Hr.Application.Cqrs.Commands.InsuranceCommands.UpdateInsuranceContributionRule;
using Anemoi.Hr.Application.Cqrs.Commands.InsuranceCommands.UpdateInsuranceRuleSet;
using Anemoi.Hr.Application.Cqrs.Queries.InsuranceQueries.GetInsuranceCalculationSnapshotDetail;
using Anemoi.Hr.Application.Cqrs.Queries.InsuranceQueries.GetInsuranceCalculationSnapshots;
using Anemoi.Hr.Application.Cqrs.Queries.InsuranceQueries.GetInsuranceContributionReport;
using Anemoi.Hr.Application.Cqrs.Queries.InsuranceQueries.GetInsuranceRuleSetDetail;
using Anemoi.Hr.Application.Cqrs.Queries.InsuranceQueries.GetInsuranceRuleSets;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Insurance;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore.Query;
using OneOf;
using Xunit;

namespace Anemoi.BuildingBlock.Test;

public sealed class HrInsuranceTests
{
    private readonly FakeUnitOfWork _unitOfWork = new();

    [Fact]
    public async Task Should_Create_InsuranceRuleSet_Successfully()
    {
        var ruleSets = new List<InsuranceRuleSet>();
        var repo = new FakeRepository<InsuranceRuleSet>(ruleSets);
        var handler = new CreateInsuranceRuleSetHandler(repo, _unitOfWork);

        var command = new CreateInsuranceRuleSetCommand(
            CountryCode: "VN",
            InsuranceType: "SOCIALINSURANCE",
            Name: "VN Social Insurance 2026",
            Currency: "VND",
            EffectiveFrom: new DateOnly(2026, 1, 1),
            EffectiveTo: null,
            CreatedBy: "user1");

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsT0);
        var response = result.AsT0;
        Assert.NotNull(response.InsuranceRuleSetId);
        Assert.Single(ruleSets);
        Assert.Equal("VN", ruleSets[0].CountryCode);
        Assert.Equal("SOCIALINSURANCE", ruleSets[0].InsuranceType);
        Assert.Equal("Draft", ruleSets[0].Status);
        Assert.Equal(1, ruleSets[0].Version);
    }

    [Fact]
    public async Task Should_Create_InsuranceContributionRule_Successfully()
    {
        var ruleSetId = new InsuranceRuleSetId(Guid.NewGuid());
        var ruleSet = new InsuranceRuleSet
        {
            Id = ruleSetId,
            CountryCode = "VN",
            InsuranceType = "SOCIALINSURANCE",
            Name = "Test",
            Currency = "VND",
            EffectiveFrom = new DateOnly(2026, 1, 1),
            Status = "Draft",
            Version = 1,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "user"
        };

        var ruleSets = new List<InsuranceRuleSet> { ruleSet };
        var rules = new List<InsuranceContributionRule>();
        var ruleSetRepo = new FakeRepository<InsuranceRuleSet>(ruleSets);
        var ruleRepo = new FakeRepository<InsuranceContributionRule>(rules);
        var handler = new CreateInsuranceContributionRuleHandler(ruleSetRepo, ruleRepo, _unitOfWork);

        var command = new CreateInsuranceContributionRuleCommand(
            RuleSetId: ruleSetId.Value.ToString(),
            ContributionType: "Standard",
            EmployeeRate: 0.08m,
            EmployerRate: 0.175m,
            CeilingAmount: null,
            MinimumAmount: null,
            SalaryBasis: "GrossSalary",
            SortOrder: 1);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsT0);
        var response = result.AsT0;
        Assert.NotNull(response.InsuranceContributionRuleId);
        Assert.Single(rules);
        Assert.Equal(0.08m, rules[0].EmployeeRate);
        Assert.Equal(0.175m, rules[0].EmployerRate);
    }

    [Fact]
    public async Task Should_Reject_Edit_Active_RuleSet()
    {
        var ruleSetId = new InsuranceRuleSetId(Guid.NewGuid());
        var ruleSet = new InsuranceRuleSet
        {
            Id = ruleSetId,
            CountryCode = "VN",
            InsuranceType = "SOCIALINSURANCE",
            Name = "Test",
            Currency = "VND",
            EffectiveFrom = new DateOnly(2026, 1, 1),
            Status = "Active",
            Version = 1,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "user"
        };

        var ruleSets = new List<InsuranceRuleSet> { ruleSet };
        var repo = new FakeRepository<InsuranceRuleSet>(ruleSets);
        var handler = new UpdateInsuranceRuleSetHandler(repo, _unitOfWork);

        var command = new UpdateInsuranceRuleSetCommand(
            Id: ruleSetId.Value.ToString(),
            Name: "Updated",
            Currency: "VND",
            EffectiveFrom: new DateOnly(2026, 1, 1),
            EffectiveTo: null,
            UpdatedBy: "user");

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsT1);
    }

    [Fact]
    public async Task Should_Reject_Overlapping_Active_RuleSet()
    {
        var existingId = new InsuranceRuleSetId(Guid.NewGuid());
        var existing = new InsuranceRuleSet
        {
            Id = existingId,
            CountryCode = "VN",
            InsuranceType = "SOCIALINSURANCE",
            Name = "Existing Active",
            Currency = "VND",
            EffectiveFrom = new DateOnly(2026, 1, 1),
            EffectiveTo = new DateOnly(2027, 1, 1),
            Status = "Active",
            Version = 1,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "user"
        };

        var draftId = new InsuranceRuleSetId(Guid.NewGuid());
        var draft = new InsuranceRuleSet
        {
            Id = draftId,
            CountryCode = "VN",
            InsuranceType = "SOCIALINSURANCE",
            Name = "Draft to activate",
            Currency = "VND",
            EffectiveFrom = new DateOnly(2026, 6, 1),
            EffectiveTo = new DateOnly(2027, 6, 1),
            Status = "Draft",
            Version = 2,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "user",
            ContributionRules =
            [
                new InsuranceContributionRule
                {
                    Id = new InsuranceContributionRuleId(Guid.NewGuid()),
                    RuleSetId = draftId,
                    ContributionType = "Standard",
                    EmployeeRate = 0.08m,
                    EmployerRate = 0.175m,
                    SalaryBasis = "GrossSalary",
                    SortOrder = 1
                }
            ]
        };

        var ruleSets = new List<InsuranceRuleSet> { existing, draft };
        var rules = draft.ContributionRules;
        var auditLogs = new List<InsuranceAuditLog>();
        var ruleSetRepo = new FakeRepository<InsuranceRuleSet>(ruleSets);
        var ruleRepo = new FakeRepository<InsuranceContributionRule>(rules);
        var auditRepo = new FakeRepository<InsuranceAuditLog>(auditLogs);
        var handler = new ActivateInsuranceRuleSetHandler(ruleSetRepo, ruleRepo, auditRepo, _unitOfWork);

        var command = new ActivateInsuranceRuleSetCommand(
            Id: draftId.Value.ToString(),
            ActivatedBy: "user");

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsT1);
    }

    [Fact]
    public async Task Should_Calculate_Insurance_With_Employee_And_Employer_Rates()
    {
        var ruleSetId = new InsuranceRuleSetId(Guid.NewGuid());
        var ruleSet = new InsuranceRuleSet
        {
            Id = ruleSetId,
            CountryCode = "VN",
            InsuranceType = "SOCIALINSURANCE",
            Name = "VN SI 2026",
            Currency = "VND",
            EffectiveFrom = new DateOnly(2026, 1, 1),
            Status = "Active",
            Version = 1
        };

        var rule = new InsuranceContributionRule
        {
            Id = new InsuranceContributionRuleId(Guid.NewGuid()),
            RuleSetId = ruleSetId,
            ContributionType = "Standard",
            EmployeeRate = 0.08m,
            EmployerRate = 0.175m,
            CeilingAmount = null,
            MinimumAmount = null,
            SalaryBasis = "GrossSalary",
            SortOrder = 1,
            RuleSet = ruleSet
        };

        var ruleSets = new List<InsuranceRuleSet> { ruleSet };
        var rules = new List<InsuranceContributionRule> { rule };
        var snapshots = new List<InsuranceCalculationSnapshot>();
        var snapshotItems = new List<InsuranceCalculationSnapshotItem>();

        var ruleSetRepo = new FakeRepository<InsuranceRuleSet>(ruleSets);
        var ruleRepo = new FakeRepository<InsuranceContributionRule>(rules);
        var snapshotRepo = new FakeRepository<InsuranceCalculationSnapshot>(snapshots);
        var handler = new CalculateInsuranceHandler(ruleSetRepo, ruleRepo, snapshotRepo, _unitOfWork);

        var employeeId = Guid.NewGuid();
        var command = new CalculateInsuranceCommand(
            EmployeeId: employeeId.ToString(),
            CountryCode: "VN",
            InsuranceType: "SOCIALINSURANCE",
            GrossSalarySnapshot: 10000000m,
            ContractSalarySnapshot: null,
            Currency: "VND",
            PeriodStart: new DateOnly(2026, 1, 1),
            PeriodEnd: new DateOnly(2026, 1, 31),
            SourceModule: "Manual",
            SourceReferenceId: null,
            CalculatedBy: "user");

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsT0, $"Expected success but got error: {result.Value}");
        var response = result.AsT0;
        Assert.Single(snapshots);

        var snapshot = snapshots[0];
        Assert.Single(snapshot.Items);
        var item = snapshot.Items[0];

        Assert.Equal(10000000m, snapshot.InsurableSalarySnapshot);
        Assert.Equal(800000m, snapshot.TotalEmployeeContribution);  // 10M * 0.08
        Assert.Equal(1750000m, snapshot.TotalEmployerContribution); // 10M * 0.175
        Assert.Equal(2550000m, snapshot.TotalContribution);

        Assert.Equal(800000m, item.EmployeeAmount);
        Assert.Equal(1750000m, item.EmployerAmount);
        Assert.Equal(2550000m, item.TotalAmount);
        Assert.Equal("Standard", item.ContributionType);
    }

    [Fact]
    public async Task Should_Apply_Ceiling_To_Calculation()
    {
        var ruleSetId = new InsuranceRuleSetId(Guid.NewGuid());
        var ruleSet = new InsuranceRuleSet
        {
            Id = ruleSetId,
            CountryCode = "VN",
            InsuranceType = "SOCIALINSURANCE",
            Name = "VN SI 2026",
            Currency = "VND",
            EffectiveFrom = new DateOnly(2026, 1, 1),
            Status = "Active",
            Version = 1
        };

        var rule = new InsuranceContributionRule
        {
            Id = new InsuranceContributionRuleId(Guid.NewGuid()),
            RuleSetId = ruleSetId,
            ContributionType = "Standard",
            EmployeeRate = 0.08m,
            EmployerRate = 0.175m,
            CeilingAmount = 8000000m,
            MinimumAmount = null,
            SalaryBasis = "GrossSalary",
            SortOrder = 1
        };

        var ruleSets = new List<InsuranceRuleSet> { ruleSet };
        var rules = new List<InsuranceContributionRule> { rule };
        var snapshots = new List<InsuranceCalculationSnapshot>();
        var snapshotItems = new List<InsuranceCalculationSnapshotItem>();

        var ruleSetRepo = new FakeRepository<InsuranceRuleSet>(ruleSets);
        var ruleRepo = new FakeRepository<InsuranceContributionRule>(rules);
        var snapshotRepo = new FakeRepository<InsuranceCalculationSnapshot>(snapshots);
        var handler = new CalculateInsuranceHandler(ruleSetRepo, ruleRepo, snapshotRepo, _unitOfWork);

        var command = new CalculateInsuranceCommand(
            EmployeeId: Guid.NewGuid().ToString(),
            CountryCode: "VN",
            InsuranceType: "SOCIALINSURANCE",
            GrossSalarySnapshot: 10000000m,
            ContractSalarySnapshot: null,
            Currency: "VND",
            PeriodStart: new DateOnly(2026, 1, 1),
            PeriodEnd: new DateOnly(2026, 1, 31),
            SourceModule: "Manual",
            SourceReferenceId: null,
            CalculatedBy: "user");

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsT0);
        var snapshot = snapshots[0];
        Assert.Equal(10000000m, snapshot.InsurableSalarySnapshot);  // stores raw GrossSalarySnapshot
        Assert.Equal(640000m, snapshot.TotalEmployeeContribution);    // 8M * 0.08
        Assert.Equal(1400000m, snapshot.TotalEmployerContribution);   // 8M * 0.175
    }

    [Fact]
    public async Task Should_Apply_MinimumAmount_To_Calculation()
    {
        var ruleSetId = new InsuranceRuleSetId(Guid.NewGuid());
        var ruleSet = new InsuranceRuleSet
        {
            Id = ruleSetId,
            CountryCode = "VN",
            InsuranceType = "SOCIALINSURANCE",
            Name = "VN SI 2026",
            Currency = "VND",
            EffectiveFrom = new DateOnly(2026, 1, 1),
            Status = "Active",
            Version = 1
        };

        var rule = new InsuranceContributionRule
        {
            Id = new InsuranceContributionRuleId(Guid.NewGuid()),
            RuleSetId = ruleSetId,
            ContributionType = "Standard",
            EmployeeRate = 0.08m,
            EmployerRate = 0.175m,
            CeilingAmount = null,
            MinimumAmount = 5000000m,
            SalaryBasis = "GrossSalary",
            SortOrder = 1
        };

        var ruleSets = new List<InsuranceRuleSet> { ruleSet };
        var rules = new List<InsuranceContributionRule> { rule };
        var snapshots = new List<InsuranceCalculationSnapshot>();
        var snapshotItems = new List<InsuranceCalculationSnapshotItem>();

        var ruleSetRepo = new FakeRepository<InsuranceRuleSet>(ruleSets);
        var ruleRepo = new FakeRepository<InsuranceContributionRule>(rules);
        var snapshotRepo = new FakeRepository<InsuranceCalculationSnapshot>(snapshots);
        var handler = new CalculateInsuranceHandler(ruleSetRepo, ruleRepo, snapshotRepo, _unitOfWork);

        var command = new CalculateInsuranceCommand(
            EmployeeId: Guid.NewGuid().ToString(),
            CountryCode: "VN",
            InsuranceType: "SOCIALINSURANCE",
            GrossSalarySnapshot: 3000000m,
            ContractSalarySnapshot: null,
            Currency: "VND",
            PeriodStart: new DateOnly(2026, 1, 1),
            PeriodEnd: new DateOnly(2026, 1, 31),
            SourceModule: "Manual",
            SourceReferenceId: null,
            CalculatedBy: "user");

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsT0);
        var snapshot = snapshots[0];
        Assert.Equal(3000000m, snapshot.InsurableSalarySnapshot);  // stores raw GrossSalarySnapshot
        Assert.Equal(400000m, snapshot.TotalEmployeeContribution);
        Assert.Equal(875000m, snapshot.TotalEmployerContribution);
    }

    [Fact]
    public async Task Should_Round_Calculation_Result_Correctly()
    {
        var ruleSetId = new InsuranceRuleSetId(Guid.NewGuid());
        var ruleSet = new InsuranceRuleSet
        {
            Id = ruleSetId,
            CountryCode = "VN",
            InsuranceType = "HEALTHINSURANCE",
            Name = "VN HI 2026",
            Currency = "VND",
            EffectiveFrom = new DateOnly(2026, 1, 1),
            Status = "Active",
            Version = 1
        };

        var rule = new InsuranceContributionRule
        {
            Id = new InsuranceContributionRuleId(Guid.NewGuid()),
            RuleSetId = ruleSetId,
            ContributionType = "Standard",
            EmployeeRate = 0.015m,
            EmployerRate = 0.03m,
            CeilingAmount = null,
            MinimumAmount = null,
            SalaryBasis = "GrossSalary",
            SortOrder = 1
        };

        var ruleSets = new List<InsuranceRuleSet> { ruleSet };
        var rules = new List<InsuranceContributionRule> { rule };
        var snapshots = new List<InsuranceCalculationSnapshot>();
        var snapshotItems = new List<InsuranceCalculationSnapshotItem>();

        var ruleSetRepo = new FakeRepository<InsuranceRuleSet>(ruleSets);
        var ruleRepo = new FakeRepository<InsuranceContributionRule>(rules);
        var snapshotRepo = new FakeRepository<InsuranceCalculationSnapshot>(snapshots);
        var handler = new CalculateInsuranceHandler(ruleSetRepo, ruleRepo, snapshotRepo, _unitOfWork);

        var command = new CalculateInsuranceCommand(
            EmployeeId: Guid.NewGuid().ToString(),
            CountryCode: "VN",
            InsuranceType: "HEALTHINSURANCE",
            GrossSalarySnapshot: 7777777m,
            ContractSalarySnapshot: null,
            Currency: "VND",
            PeriodStart: new DateOnly(2026, 1, 1),
            PeriodEnd: new DateOnly(2026, 1, 31),
            SourceModule: "Manual",
            SourceReferenceId: null,
            CalculatedBy: "user");

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsT0);
        var snapshot = snapshots[0];
        var expectedEmployee = Math.Round(7777777m * 0.015m, 2, MidpointRounding.AwayFromZero);
        var expectedEmployer = Math.Round(7777777m * 0.03m, 2, MidpointRounding.AwayFromZero);
        Assert.Equal(expectedEmployee, snapshot.TotalEmployeeContribution);
        Assert.Equal(expectedEmployer, snapshot.TotalEmployerContribution);
    }

    [Fact]
    public async Task Should_Create_Snapshot_With_Items()
    {
        var ruleSetId = new InsuranceRuleSetId(Guid.NewGuid());
        var ruleSet = new InsuranceRuleSet
        {
            Id = ruleSetId,
            CountryCode = "VN",
            InsuranceType = "SOCIALINSURANCE",
            Name = "VN SI 2026",
            Currency = "VND",
            EffectiveFrom = new DateOnly(2026, 1, 1),
            Status = "Active",
            Version = 2
        };

        var rules = new List<InsuranceContributionRule>
        {
            new()
            {
                Id = new InsuranceContributionRuleId(Guid.NewGuid()),
                RuleSetId = ruleSetId,
                ContributionType = "Standard",
                EmployeeRate = 0.08m,
                EmployerRate = 0.175m,
                SalaryBasis = "GrossSalary",
                SortOrder = 1
            }
        };

        var ruleSets = new List<InsuranceRuleSet> { ruleSet };
        var snapshots = new List<InsuranceCalculationSnapshot>();
        var snapshotItems = new List<InsuranceCalculationSnapshotItem>();

        var ruleSetRepo = new FakeRepository<InsuranceRuleSet>(ruleSets);
        var ruleRepo = new FakeRepository<InsuranceContributionRule>(rules);
        var snapshotRepo = new FakeRepository<InsuranceCalculationSnapshot>(snapshots);
        var handler = new CalculateInsuranceHandler(ruleSetRepo, ruleRepo, snapshotRepo, _unitOfWork);

        var command = new CalculateInsuranceCommand(
            EmployeeId: Guid.NewGuid().ToString(),
            CountryCode: "VN",
            InsuranceType: "SOCIALINSURANCE",
            GrossSalarySnapshot: 10000000m,
            ContractSalarySnapshot: null,
            Currency: "VND",
            PeriodStart: new DateOnly(2026, 1, 1),
            PeriodEnd: new DateOnly(2026, 1, 31),
            SourceModule: "Payroll",
            SourceReferenceId: "PR-2026-001",
            CalculatedBy: "user");

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsT0);
        var snapshot = snapshots[0];
        Assert.Equal(2, snapshot.RuleSetVersion);
        Assert.Equal("Payroll", snapshot.SourceModule);
        Assert.Equal("PR-2026-001", snapshot.SourceReferenceId);
        Assert.NotNull(snapshot.RuleSetSnapshotJson);
        Assert.NotNull(snapshot.CalculationResultJson);

        Assert.Single(snapshot.Items);
        var item = snapshot.Items[0];
        Assert.Equal(0.08m, item.EmployeeRate);
        Assert.Equal(0.175m, item.EmployerRate);
        Assert.Equal(10000000m, item.ContributionBase);
    }

    [Fact]
    public async Task Should_Get_SnapshotDetail_With_Items()
    {
        var snapshotId = new InsuranceCalculationSnapshotId(Guid.NewGuid());
        var ruleSetId = new InsuranceRuleSetId(Guid.NewGuid());

        var snapshot = new InsuranceCalculationSnapshot
        {
            Id = snapshotId,
            CountryCode = "VN",
            InsuranceType = "SOCIALINSURANCE",
            RuleSetId = ruleSetId,
            RuleSetVersion = 1,
            Currency = "VND",
            CalculationPeriodStart = new DateOnly(2026, 1, 1),
            CalculationPeriodEnd = new DateOnly(2026, 1, 31),
            InsurableSalarySnapshot = 10000000m,
            TotalEmployeeContribution = 800000m,
            TotalEmployerContribution = 1750000m,
            TotalContribution = 2550000m,
            RuleSetSnapshotJson = "{}",
            CalculationResultJson = "{}",
            CalculatedAt = DateTime.UtcNow,
            CalculatedBy = "user",
            SourceModule = "Manual",
            Items =
            [
                new InsuranceCalculationSnapshotItem
                {
                    Id = new InsuranceCalculationSnapshotItemId(Guid.NewGuid()),
                    SnapshotId = snapshotId,
                    InsuranceType = "SOCIALINSURANCE",
                    ContributionType = "Standard",
                    ContributionBase = 10000000m,
                    EmployeeRate = 0.08m,
                    EmployerRate = 0.175m,
                    EmployeeAmount = 800000m,
                    EmployerAmount = 1750000m,
                    TotalAmount = 2550000m,
                    SortOrder = 1
                }
            ]
        };

        var snapshots = new List<InsuranceCalculationSnapshot> { snapshot };
        var snapshotRepo = new FakeRepository<InsuranceCalculationSnapshot>(snapshots);
        var mapper = new InsuranceMapper();
        var handler = new GetInsuranceCalculationSnapshotDetailHandler(snapshotRepo, mapper);

        var query = new GetInsuranceCalculationSnapshotDetailQuery(snapshotId.Value.ToString());
        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsT0);
        var response = result.AsT0;
        Assert.NotNull(response);
        Assert.Equal("VN", response.CountryCode);
        Assert.Single(response.Items);
        var item = response.Items[0];
        Assert.Equal("Standard", item.ContributionType);
        Assert.Equal(10000000m, item.ContributionBase);
        Assert.Equal(800000m, item.EmployeeAmount);
        Assert.Equal(1750000m, item.EmployerAmount);
        Assert.Equal(2550000m, item.TotalAmount);
    }

    [Fact]
    public async Task Should_Query_Snapshots_By_EmployeeId()
    {
        var employeeId = Guid.NewGuid();
        var ruleSetId = new InsuranceRuleSetId(Guid.NewGuid());

        var snapshots = new List<InsuranceCalculationSnapshot>
        {
            new()
            {
                Id = new InsuranceCalculationSnapshotId(Guid.NewGuid()),
                EmployeeId = new EmployeeId(employeeId),
                CountryCode = "VN",
                InsuranceType = "SOCIALINSURANCE",
                RuleSetId = ruleSetId,
                RuleSetVersion = 1,
                Currency = "VND",
                CalculationPeriodStart = new DateOnly(2026, 1, 1),
                CalculationPeriodEnd = new DateOnly(2026, 1, 31),
                InsurableSalarySnapshot = 10000000m,
                TotalEmployeeContribution = 800000m,
                TotalEmployerContribution = 1750000m,
                TotalContribution = 2550000m,
                RuleSetSnapshotJson = "{}",
                CalculationResultJson = "{}",
                CalculatedAt = DateTime.UtcNow,
                CalculatedBy = "user",
                SourceModule = "Manual"
            }
        };

        var snapshotRepo = new FakeRepository<InsuranceCalculationSnapshot>(snapshots);
        var mapper = new InsuranceMapper();
        var handler = new GetInsuranceCalculationSnapshotsHandler(snapshotRepo, mapper);

        var query = new GetInsuranceCalculationSnapshotsQuery(
            EmployeeId: employeeId.ToString(),
            InsuranceType: null,
            SourceModule: null,
            SourceReferenceId: null);

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.Single(result);
        Assert.Equal(800000m, result.First().TotalEmployeeContribution);
    }

    [Fact]
    public async Task Should_Query_Contribution_Report()
    {
        var ruleSetId = new InsuranceRuleSetId(Guid.NewGuid());
        var employeeId = Guid.NewGuid();

        var snapshots = new List<InsuranceCalculationSnapshot>
        {
            new()
            {
                Id = new InsuranceCalculationSnapshotId(Guid.NewGuid()),
                EmployeeId = new EmployeeId(employeeId),
                CountryCode = "VN",
                InsuranceType = "SOCIALINSURANCE",
                RuleSetId = ruleSetId,
                RuleSetVersion = 1,
                Currency = "VND",
                CalculationPeriodStart = new DateOnly(2026, 1, 1),
                CalculationPeriodEnd = new DateOnly(2026, 1, 31),
                InsurableSalarySnapshot = 10000000m,
                TotalEmployeeContribution = 800000m,
                TotalEmployerContribution = 1750000m,
                TotalContribution = 2550000m,
                RuleSetSnapshotJson = "{}",
                CalculationResultJson = "{}",
                CalculatedAt = DateTime.UtcNow,
                CalculatedBy = "user",
                SourceModule = "Payroll"
            }
        };

        var snapshotRepo = new FakeRepository<InsuranceCalculationSnapshot>(snapshots);
        var mapper = new InsuranceMapper();
        var handler = new GetInsuranceContributionReportHandler(snapshotRepo, mapper);

        var query = new GetInsuranceContributionReportQuery(
            PeriodStart: new DateOnly(2026, 1, 1),
            PeriodEnd: new DateOnly(2026, 12, 31),
            CountryCode: null,
            InsuranceType: null);

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.Single(result);
        var report = result.First();
        Assert.Equal("VN", report.CountryCode);
        Assert.Equal("SOCIALINSURANCE", report.InsuranceType);
        Assert.Equal(2550000m, report.TotalContribution);
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public Task<OneOf<None, Exception>> SaveChangesAsync(CancellationToken token = default)
        {
            return Task.FromResult<OneOf<None, Exception>>(None.Value);
        }
    }

    private sealed class FakeRepository<T>(List<T> initialItems) : ISqlRepository<T>
        where T : class
    {
        private readonly List<T> _items = initialItems;

        public IQueryable<T> GetQueryable(Expression<Func<T, bool>> conditionExpression = null)
        {
            var queryable = _items.AsQueryable();
            if (conditionExpression is not null)
                queryable = queryable.Where(conditionExpression);
            return new TestAsyncEnumerable<T>(queryable);
        }

        public IQueryable<T> GetQueryableFromRawQuery(string sql, params object[] parameters) => GetQueryable();

        public Task<T> GetFirstByConditionAsync(
            Expression<Func<T, bool>> conditionExpression = null,
            Func<IQueryable<T>, IQueryable<T>> specialAction = null,
            CancellationToken token = default)
        {
            IQueryable<T> queryable = GetQueryable(conditionExpression);
            if (specialAction is not null)
                queryable = specialAction(queryable);
            return Task.FromResult(queryable.FirstOrDefault());
        }

        public Task<bool> ExistByConditionAsync(Expression<Func<T, bool>> conditionExpression = null, CancellationToken token = default)
        {
            return Task.FromResult(GetQueryable(conditionExpression).Any());
        }

        public Task<List<T>> GetManyByConditionAsync(
            Expression<Func<T, bool>> conditionExpression = null,
            Func<IQueryable<T>, IQueryable<T>> specialAction = null,
            CancellationToken token = default)
        {
            IQueryable<T> queryable = GetQueryable(conditionExpression);
            if (specialAction is not null)
                queryable = specialAction(queryable);
            return Task.FromResult(queryable.ToList());
        }

        public Task<Pagination<T>> GetManyByConditionWithPaginationAsync(Expression<Func<T, bool>> conditionExpression = null, Func<IQueryable<T>, IQueryable<T>> specialAction = null, CancellationToken token = default) => throw new NotSupportedException();
        public Task<long> CountByConditionAsync(Expression<Func<T, bool>> conditionExpression = null, Func<IQueryable<T>, IQueryable<T>> specialAction = null, CancellationToken token = default) => Task.FromResult((long)GetQueryable(conditionExpression).Count());

        public Task<OneOf<T, Exception>> CreateOneAsync(T item, CancellationToken token = default)
        {
            _items.Add(item);
            return Task.FromResult<OneOf<T, Exception>>(item);
        }

        public Task<OneOf<None, Exception>> CreateManyAsync(List<T> items, CancellationToken token = default)
        {
            _items.AddRange(items);
            return Task.FromResult<OneOf<None, Exception>>(None.Value);
        }

        public Task<OneOf<None, Exception>> RemoveOneAsync(OneOf<T, Expression<Func<T, bool>>> itemOrFilter, CancellationToken token = default)
        {
            if (itemOrFilter.IsT0)
            {
                _items.Remove(itemOrFilter.AsT0);
            }
            return Task.FromResult<OneOf<None, Exception>>(None.Value);
        }

        public Task<OneOf<None, Exception>> RemoveManyAsync(OneOf<List<T>, Expression<Func<T, bool>>> itemsOrFilter, CancellationToken token = default) => throw new NotSupportedException();
    }

    private sealed class TestAsyncQueryProvider<TElement>(IQueryProvider inner) : IAsyncQueryProvider
    {
        public IQueryable CreateQuery(Expression expression) => new TestAsyncEnumerable<TElement>(expression);
        public IQueryable<TElement1> CreateQuery<TElement1>(Expression expression) => new TestAsyncEnumerable<TElement1>(expression);
        public object Execute(Expression expression) => inner.Execute(expression);
        public TResult Execute<TResult>(Expression expression) => inner.Execute<TResult>(expression);
        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
        {
            var expectedResultType = typeof(TResult).GetGenericArguments().FirstOrDefault();
            if (expectedResultType is null)
                return Execute<TResult>(expression);

            var executeMethod = typeof(IQueryProvider)
                .GetMethods()
                .Single(x => x.Name == nameof(IQueryProvider.Execute) && x.IsGenericMethod)
                .MakeGenericMethod(expectedResultType);
            var result = executeMethod.Invoke(inner, [expression]);
            return (TResult)typeof(Task)
                .GetMethod(nameof(Task.FromResult))!
                .MakeGenericMethod(expectedResultType)
                .Invoke(null, [result])!;
        }
    }

    private sealed class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable) { }
        public TestAsyncEnumerable(Expression expression) : base(expression) { }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
        }

        IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
    }

    private sealed class TestAsyncEnumerator<T>(IEnumerator<T> inner) : IAsyncEnumerator<T>
    {
        public T Current => inner.Current;
        public ValueTask DisposeAsync()
        {
            inner.Dispose();
            return ValueTask.CompletedTask;
        }

        public ValueTask<bool> MoveNextAsync() => new(inner.MoveNext());
    }
}
