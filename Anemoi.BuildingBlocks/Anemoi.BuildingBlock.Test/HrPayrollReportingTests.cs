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
