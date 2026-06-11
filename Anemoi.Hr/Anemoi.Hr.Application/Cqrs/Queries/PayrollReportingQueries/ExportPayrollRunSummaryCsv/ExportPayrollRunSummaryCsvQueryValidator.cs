#nullable enable

using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Domain.Payroll;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Queries.PayrollReportingQueries.ExportPayrollRunSummaryCsv;

public sealed class ExportPayrollRunSummaryCsvQueryValidator : AbstractValidator<ExportPayrollRunSummaryCsvQuery>
{
    public ExportPayrollRunSummaryCsvQueryValidator()
    {
        RuleFor(x => x.SortDirection)
            .Must(x => string.IsNullOrWhiteSpace(x) || x.Equals("asc", StringComparison.OrdinalIgnoreCase) || x.Equals("desc", StringComparison.OrdinalIgnoreCase))
            .WithMessage(HrBusinessErrorCodes.ReportSortDirectionInvalid);
        RuleFor(x => x.Status)
            .Must(x => string.IsNullOrWhiteSpace(x) || Enum.TryParse<PayrollRunStatus>(x, true, out _))
            .WithMessage(HrBusinessErrorCodes.ReportStatusInvalid);
        RuleFor(x => x.FinalizedTo)
            .GreaterThanOrEqualTo(x => x.FinalizedFrom)
            .When(x => x.FinalizedFrom.HasValue && x.FinalizedTo.HasValue)
            .WithMessage(HrBusinessErrorCodes.ReportDateRangeInvalid);
    }
}
