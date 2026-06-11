using System.Text;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Queries.PayrollReportingQueries.ExportPayslipSummaryCsv;
using Anemoi.Hr.Application.Cqrs.Queries.PayrollReportingQueries.GetPayrollRunSummaryReport;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Infrastructure.Reporting;
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

    private sealed record CsvRow(string EmployeeName, string Description, decimal Amount);
}
