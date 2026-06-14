using System.Text;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Pipelines;
using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Queries.PayrollReportingQueries.ExportPayslipSummaryCsv;
using Anemoi.Hr.Application.Cqrs.Queries.PayrollReportingQueries.GetPayrollRunSummaryReport;
using Anemoi.Hr.Application.Cqrs.Queries.PayrollReportingQueries.Shared;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Infrastructure.Reporting;
using Anemoi.Hr.Infrastructure.Services;
using Anemoi.Hr.Infrastructure.Installers;
using Anemoi.Hr.Domain.Reporting;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Departments;
using Anemoi.Hr.Domain.Positions;
using Anemoi.Hr.Domain.Payroll;
using Anemoi.Hr.Domain.Taxation;
using Anemoi.Hr.Domain.Insurance;
using Anemoi.Hr.ModelIds.ModelIds;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Anemoi.BuildingBlock.Test;

public sealed class HrPayrollReportingTests
{
    [Fact]
    public void PayrollRunSummary_DoesNotExposePaidLeaveDays()
    {
        Assert.Null(typeof(PayrollRunSummaryReportItem).GetProperty("PaidLeaveDays"));
    }

    [Fact]
    public void JsonValidator_RejectsInvalidPageDateRangeDirectionAndStatus()
    {
        var validator = new GetPayrollRunSummaryReportQueryValidator();
        var query = new GetPayrollRunSummaryReportQuery
        {
            Page = 0,
            PageSize = 1001,
            FinalizedFrom = new DateTime(2026, 6, 2),
            FinalizedTo = new DateTime(2026, 6, 1),
            SortDirection = "sideways",
            Status = "Unknown"
        };

        var result = validator.Validate(query);

        Assert.Contains(result.Errors, x => x.ErrorMessage == HrBusinessErrorCodes.ReportPageInvalid);
        Assert.Contains(result.Errors, x => x.ErrorMessage == HrBusinessErrorCodes.ReportPageSizeInvalid);
        Assert.Contains(result.Errors, x => x.ErrorMessage == HrBusinessErrorCodes.ReportDateRangeInvalid);
        Assert.Contains(result.Errors, x => x.ErrorMessage == HrBusinessErrorCodes.ReportSortDirectionInvalid);
        Assert.Contains(result.Errors, x => x.ErrorMessage == HrBusinessErrorCodes.ReportStatusInvalid);
    }

    [Fact]
    public void ExportValidator_AcceptsValidPayslipFilters()
    {
        var validator = new ExportPayslipSummaryCsvQueryValidator();
        var query = new ExportPayslipSummaryCsvQuery
        {
            GeneratedFrom = new DateTime(2026, 6, 1),
            GeneratedTo = new DateTime(2026, 6, 2),
            SortDirection = "DESC",
            Status = "Published"
        };

        Assert.True(validator.Validate(query).IsValid);
    }

    [Fact]
    public void CsvExporter_UsesUtf8AndEscapesCsvValues()
    {
        var exporter = new CsvReportExporter();
        var result = exporter.ExportCsv(
            new[] { new CsvRow("Doe, Jane", "A \"quoted\" value", 123.45m) },
            "report.csv");
        var csv = Encoding.UTF8.GetString(result.Content).TrimStart('\uFEFF');

        Assert.Equal("text/csv; charset=utf-8", result.ContentType);
        Assert.Equal("report.csv", result.FileName);
        Assert.Contains("\"Doe, Jane\"", csv);
        Assert.Contains("\"A \"\"quoted\"\" value\"", csv);
        Assert.Contains("123.45", csv);
    }

    [Fact]
    public void CurrentUser_ReturnsEmpty_WhenHttpContextIsUnavailable()
    {
        var currentUser = new CurrentUser(new HttpContextAccessor());

        Assert.Equal(string.Empty, currentUser.UserId);
    }

    [Fact]
    public void CurrentUser_ReturnsEmpty_WhenUserIdClaimIsUnavailable()
    {
        var accessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };
        var currentUser = new CurrentUser(accessor);

        Assert.Equal(string.Empty, currentUser.UserId);
    }

    [Fact]
    public async Task ExportService_FailsSafely_WhenCurrentUserCannotBeResolved()
    {
        var service = new PayrollReportExportService(
            null!,
            new StubCurrentUser(string.Empty),
            null!,
            null!);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.ExportAsync(
            Array.Empty<CsvRow>(),
            ReportTypes.PayrollRunSummary,
            new { },
            CancellationToken.None));

        Assert.Equal(HrBusinessErrorCodes.ReportExportPermissionDenied, exception.Message);
    }

    [Fact]
    public async Task ExportService_RejectsExportsAboveMvpRowLimit()
    {
        var service = new PayrollReportExportService(
            null!,
            new StubCurrentUser("authenticated-user-id"),
            null!,
            null!);
        var records = Enumerable.Repeat(
            new CsvRow("Employee", "Description", 1m),
            ReportExportLimits.MaxRows + 1).ToArray();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.ExportAsync(
            records,
            ReportTypes.PayrollRunSummary,
            new { },
            CancellationToken.None));

        Assert.Equal(HrBusinessErrorCodes.ReportExportLimitExceeded, exception.Message);
    }

    [Fact]
    public void PipelineInstaller_RegistersSingleValidationBehavior()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        new PipelineInstaller().InstallerServices(services, configuration);

        Assert.Single(services, x =>
            x.ServiceType.IsGenericTypeDefinition &&
            x.ServiceType == typeof(IPipelineBehavior<,>) &&
            x.ImplementationType == typeof(ValidationBehavior<,>));
    }

    [Fact]
    public void ServiceInstaller_RegistersReportExportAuditLogRepository()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        new ServiceInstaller().InstallerServices(services, configuration);

        Assert.Contains(services, x =>
            x.ServiceType == typeof(ISqlRepository<ReportExportAuditLog>));
    }

    [Fact]
    public async Task ValidationBehavior_InvokesEachRegisteredValidatorOnce()
    {
        var validator = new CountingValidator();
        var behavior = new ValidationBehavior<ValidationRequest, string>([validator]);
        var nextCalls = 0;

        var response = await behavior.Handle(
            new ValidationRequest(),
            () =>
            {
                nextCalls++;
                return Task.FromResult("validated");
            },
            CancellationToken.None);

        Assert.Equal("validated", response);
        Assert.Equal(1, validator.InvocationCount);
        Assert.Equal(1, nextCalls);
    }

    [Fact]
    public void BuildPayrollCostSummary_GroupsAndSumsCorrectly()
    {
        var periodId = new PayrollPeriodId(Guid.NewGuid());
        var run1 = new PayrollRun { Id = new PayrollRunId(Guid.NewGuid()), PayrollPeriodId = periodId, EmployeeId = new EmployeeId(Guid.NewGuid()), EmployeeCode = "EMP001", EmployeeName = "Emp 1", GrossAmount = 1000m, NetAmount = 800m, TotalDeductionAmount = 100m, CalculatedAt = DateTime.UtcNow, CalculatedBy = "test" };
        var run2 = new PayrollRun { Id = new PayrollRunId(Guid.NewGuid()), PayrollPeriodId = periodId, EmployeeId = new EmployeeId(Guid.NewGuid()), EmployeeCode = "EMP002", EmployeeName = "Emp 2", GrossAmount = 2000m, NetAmount = 1600m, TotalDeductionAmount = 200m, CalculatedAt = DateTime.UtcNow, CalculatedBy = "test" };
        
        run1.SubmitForApproval("actor", DateTime.UtcNow); run1.Approve("actor", DateTime.UtcNow); run1.FinalizeRun("actor", DateTime.UtcNow);
        run2.SubmitForApproval("actor", DateTime.UtcNow); run2.Approve("actor", DateTime.UtcNow); run2.FinalizeRun("actor", DateTime.UtcNow);

        var runs = new List<PayrollRun> { run1, run2 }.AsQueryable();

        var periods = new List<PayrollPeriod>
        {
            new() { Id = periodId, PeriodCode = "PERIOD_01", StartDate = new DateOnly(2026, 6, 1), EndDate = new DateOnly(2026, 6, 30) }
        }.AsQueryable();

        var employees = new List<Employee>().AsQueryable();
        var taxSnapshots = new List<TaxCalculationSnapshot>
        {
            new() { Id = new TaxCalculationSnapshotId(Guid.NewGuid()), PayrollRunId = run1.Id, TaxableIncomeSnapshot = 900m, TotalTaxAmount = 50m },
            new() { Id = new TaxCalculationSnapshotId(Guid.NewGuid()), PayrollRunId = run2.Id, TaxableIncomeSnapshot = 1800m, TotalTaxAmount = 100m }
        }.AsQueryable();

        var insuranceSnapshots = new List<InsuranceCalculationSnapshot>
        {
            new() { Id = new InsuranceCalculationSnapshotId(Guid.NewGuid()), EmployeeId = run1.EmployeeId, CalculationPeriodStart = new DateOnly(2026, 6, 1), CalculationPeriodEnd = new DateOnly(2026, 6, 30), TotalEmployeeContribution = 30m, TotalEmployerContribution = 70m },
            new() { Id = new InsuranceCalculationSnapshotId(Guid.NewGuid()), EmployeeId = run2.EmployeeId, CalculationPeriodStart = new DateOnly(2026, 6, 1), CalculationPeriodEnd = new DateOnly(2026, 6, 30), TotalEmployeeContribution = 60m, TotalEmployerContribution = 140m }
        }.AsQueryable();

        var filter = new PayrollReportingFilter(periodId, null, null, null, null, null, null, null);

        var result = PayrollReportingQueryExtensions.BuildPayrollCostSummary(runs, periods, employees, taxSnapshots, insuranceSnapshots, filter).ToList();

        Assert.Single(result);
        var item = result.First();
        Assert.Equal(periodId.Value, item.PayrollPeriodId);
        Assert.Equal("PERIOD_01", item.PeriodCode);
        Assert.Equal(PayrollRunStatus.Finalized, item.Status);
        Assert.Equal(2, item.EmployeeCount);
        Assert.Equal(3000m, item.TotalGrossIncome);
        Assert.Equal(2700m, item.TotalTaxableIncome);
        Assert.Equal(150m, item.TotalEmployeeTax);
        Assert.Equal(90m, item.TotalEmployeeInsurance);
        Assert.Equal(210m, item.TotalEmployerInsurance);
        Assert.Equal(300m, item.TotalDeductions);
        Assert.Equal(2400m, item.TotalNetPay);
    }

    [Fact]
    public void BuildPayrollCostDepartment_GroupsByDepartmentCorrectly()
    {
        var deptId1 = new DepartmentId(Guid.NewGuid());
        var deptId2 = new DepartmentId(Guid.NewGuid());
        var empId1 = new EmployeeId(Guid.NewGuid());
        var empId2 = new EmployeeId(Guid.NewGuid());
        var periodId = new PayrollPeriodId(Guid.NewGuid());

        var run1 = new PayrollRun { Id = new PayrollRunId(Guid.NewGuid()), PayrollPeriodId = periodId, EmployeeId = empId1, GrossAmount = 1000m, NetAmount = 800m };
        var run2 = new PayrollRun { Id = new PayrollRunId(Guid.NewGuid()), PayrollPeriodId = periodId, EmployeeId = empId2, GrossAmount = 2000m, NetAmount = 1600m };
        var runs = new List<PayrollRun> { run1, run2 }.AsQueryable();

        var periods = new List<PayrollPeriod>
        {
            new() { Id = periodId, PeriodCode = "PERIOD_01", StartDate = new DateOnly(2026, 6, 1), EndDate = new DateOnly(2026, 6, 30) }
        }.AsQueryable();

        var employees = new List<Employee>
        {
            new() { Id = empId1, PrimaryDepartmentId = deptId1 },
            new() { Id = empId2, PrimaryDepartmentId = deptId2 }
        }.AsQueryable();

        var departments = new List<Department>
        {
            new() { Id = deptId1, Name = "Engineering" },
            new() { Id = deptId2, Name = "Product" }
        }.AsQueryable();

        var taxSnapshots = new List<TaxCalculationSnapshot>().AsQueryable();
        var insuranceSnapshots = new List<InsuranceCalculationSnapshot>().AsQueryable();

        var filter = new PayrollReportingFilter(periodId, null, null, null, null, null, null, null);

        var result = PayrollReportingQueryExtensions.BuildPayrollCostDepartment(runs, periods, employees, departments, taxSnapshots, insuranceSnapshots, filter).ToList();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, x => x.DepartmentName == "Engineering" && x.TotalGrossIncome == 1000m && x.TotalNetPay == 800m);
        Assert.Contains(result, x => x.DepartmentName == "Product" && x.TotalGrossIncome == 2000m && x.TotalNetPay == 1600m);
    }

    [Fact]
    public void BuildPayrollVariance_OnlyIncludesCurrentEmployees_WhenIncludeRemovedIsFalse()
    {
        var periodIdCurr = new PayrollPeriodId(Guid.NewGuid());
        var periodIdPrev = new PayrollPeriodId(Guid.NewGuid());
        var empIdCommon = new EmployeeId(Guid.NewGuid());
        var empIdRemoved = new EmployeeId(Guid.NewGuid());

        var runCurr = new PayrollRun { Id = new PayrollRunId(Guid.NewGuid()), PayrollPeriodId = periodIdCurr, EmployeeId = empIdCommon, EmployeeCode = "EMP001", EmployeeName = "Common Emp", GrossAmount = 1200m, NetAmount = 1000m };
        var runPrev1 = new PayrollRun { Id = new PayrollRunId(Guid.NewGuid()), PayrollPeriodId = periodIdPrev, EmployeeId = empIdCommon, EmployeeCode = "EMP001", EmployeeName = "Common Emp", GrossAmount = 1000m, NetAmount = 800m };
        var runPrev2 = new PayrollRun { Id = new PayrollRunId(Guid.NewGuid()), PayrollPeriodId = periodIdPrev, EmployeeId = empIdRemoved, EmployeeCode = "EMP002", EmployeeName = "Removed Emp", GrossAmount = 500m, NetAmount = 400m };

        var runs = new List<PayrollRun> { runCurr, runPrev1, runPrev2 }.AsQueryable();

        var periods = new List<PayrollPeriod>
        {
            new() { Id = periodIdCurr, PeriodCode = "CURR", StartDate = new DateOnly(2026, 6, 1), EndDate = new DateOnly(2026, 6, 30) },
            new() { Id = periodIdPrev, PeriodCode = "PREV", StartDate = new DateOnly(2026, 5, 1), EndDate = new DateOnly(2026, 5, 31) }
        }.AsQueryable();

        var taxSnapshots = new List<TaxCalculationSnapshot>().AsQueryable();
        var insuranceSnapshots = new List<InsuranceCalculationSnapshot>().AsQueryable();

        var result = PayrollReportingQueryExtensions.BuildPayrollVariance(
            runs, periods, taxSnapshots, insuranceSnapshots,
            periodIdCurr, periodIdPrev, null, null,
            includeRemovedEmployees: false, null, null).ToList();

        Assert.Single(result);
        var item = result.First();
        Assert.Equal("EMP001", item.EmployeeCode);
        Assert.Equal(1200m, item.CurrentGrossIncome);
        Assert.Equal(1000m, item.PreviousGrossIncome);
        Assert.Equal(200m, item.GrossIncomeDifference);
        Assert.Equal(1000m, item.CurrentNetPay);
        Assert.Equal(800m, item.PreviousNetPay);
        Assert.Equal(200m, item.NetPayDifference);
    }

    [Fact]
    public void BuildPayrollVariance_IncludesAllEmployees_WhenIncludeRemovedIsTrue()
    {
        var periodIdCurr = new PayrollPeriodId(Guid.NewGuid());
        var periodIdPrev = new PayrollPeriodId(Guid.NewGuid());
        var empIdCommon = new EmployeeId(Guid.NewGuid());
        var empIdRemoved = new EmployeeId(Guid.NewGuid());

        var runCurr = new PayrollRun { Id = new PayrollRunId(Guid.NewGuid()), PayrollPeriodId = periodIdCurr, EmployeeId = empIdCommon, EmployeeCode = "EMP001", EmployeeName = "Common Emp", GrossAmount = 1200m, NetAmount = 1000m };
        var runPrev1 = new PayrollRun { Id = new PayrollRunId(Guid.NewGuid()), PayrollPeriodId = periodIdPrev, EmployeeId = empIdCommon, EmployeeCode = "EMP001", EmployeeName = "Common Emp", GrossAmount = 1000m, NetAmount = 800m };
        var runPrev2 = new PayrollRun { Id = new PayrollRunId(Guid.NewGuid()), PayrollPeriodId = periodIdPrev, EmployeeId = empIdRemoved, EmployeeCode = "EMP002", EmployeeName = "Removed Emp", GrossAmount = 500m, NetAmount = 400m };

        var runs = new List<PayrollRun> { runCurr, runPrev1, runPrev2 }.AsQueryable();

        var periods = new List<PayrollPeriod>
        {
            new() { Id = periodIdCurr, PeriodCode = "CURR", StartDate = new DateOnly(2026, 6, 1), EndDate = new DateOnly(2026, 6, 30) },
            new() { Id = periodIdPrev, PeriodCode = "PREV", StartDate = new DateOnly(2026, 5, 1), EndDate = new DateOnly(2026, 5, 31) }
        }.AsQueryable();

        var taxSnapshots = new List<TaxCalculationSnapshot>().AsQueryable();
        var insuranceSnapshots = new List<InsuranceCalculationSnapshot>().AsQueryable();

        var result = PayrollReportingQueryExtensions.BuildPayrollVariance(
            runs, periods, taxSnapshots, insuranceSnapshots,
            periodIdCurr, periodIdPrev, null, null,
            includeRemovedEmployees: true, null, null).ToList();

        Assert.Equal(2, result.Count);
        var itemCommon = result.First(x => x.EmployeeCode == "EMP001");
        Assert.Equal(1200m, itemCommon.CurrentGrossIncome);
        Assert.Equal(1000m, itemCommon.PreviousGrossIncome);
        Assert.Equal(200m, itemCommon.GrossIncomeDifference);

        var itemRemoved = result.First(x => x.EmployeeCode == "EMP002");
        Assert.Equal(0m, itemRemoved.CurrentGrossIncome);
        Assert.Equal(500m, itemRemoved.PreviousGrossIncome);
        Assert.Equal(-500m, itemRemoved.GrossIncomeDifference);
    }

    [Fact]
    public void BuildPayslipDeliveryReport_CorrectlyResolvesPdfAndEmailDelivery()
    {
        var runId = new PayrollRunId(Guid.NewGuid());
        var empId = new EmployeeId(Guid.NewGuid());
        var periodId = new PayrollPeriodId(Guid.NewGuid());

        var run = new PayrollRun { Id = runId, PayrollPeriodId = periodId, EmployeeId = empId, EmployeeCode = "EMP001", EmployeeName = "Emp 1" };
        var runs = new List<PayrollRun> { run }.AsQueryable();

        var payslip = new Payslip { Id = new PayslipId(Guid.NewGuid()), PayrollRunId = runId, EmployeeId = empId, EmployeeCode = "EMP001", EmployeeName = "Emp 1" };
        var payslips = new List<Payslip> { payslip }.AsQueryable();

        var doc = PayslipDocument.Create(payslip.Id, "payslip.pdf", "application/pdf", "path/to/file", 1024, "hash", "generator", DateTime.UtcNow, 1);
        var documents = new List<PayslipDocument> { doc }.AsQueryable();

        var email = PayslipEmailDelivery.Create(payslip.Id, doc.Id, "test@example.com", "Subject", "sender");
        email.MarkSent(DateTime.UtcNow);
        var emails = new List<PayslipEmailDelivery> { email }.AsQueryable();

        var employees = new List<Employee>().AsQueryable();
        var filter = new PayrollReportingFilter(periodId, null, null, null, null, null, null, null);

        var result = PayrollReportingQueryExtensions.BuildPayslipDeliveryReport(runs, employees, payslips, documents, emails, filter).ToList();

        Assert.Single(result);
        var item = result.First();
        Assert.Equal("EMP001", item.EmployeeCode);
        Assert.Equal("Generated", item.PayslipStatus);
        Assert.True(item.PdfGenerated);
        Assert.True(item.EmailSent);
        Assert.True(item.DownloadAvailable);
        Assert.Null(item.EmailFailureReason);
    }

    private sealed record CsvRow(string EmployeeName, string Description, decimal Amount);
    private sealed record ValidationRequest : IRequest<string>;
    private sealed record StubCurrentUser(string UserId) : ICurrentUser;

    private sealed class CountingValidator : AbstractValidator<ValidationRequest>
    {
        public int InvocationCount { get; private set; }

        public override Task<FluentValidation.Results.ValidationResult> ValidateAsync(
            ValidationContext<ValidationRequest> context,
            CancellationToken cancellation = default)
        {
            InvocationCount++;
            return base.ValidateAsync(context, cancellation);
        }
    }
}
