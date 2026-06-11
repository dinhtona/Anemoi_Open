#nullable enable

using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Domain.Payroll;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Queries.PayrollReportingQueries.ExportPayslipSummaryCsv;

public sealed class ExportPayslipSummaryCsvQueryValidator : AbstractValidator<ExportPayslipSummaryCsvQuery>
{
    public ExportPayslipSummaryCsvQueryValidator()
    {
        RuleFor(x => x.SortDirection)
            .Must(x => string.IsNullOrWhiteSpace(x) || x.Equals("asc", StringComparison.OrdinalIgnoreCase) || x.Equals("desc", StringComparison.OrdinalIgnoreCase))
            .WithMessage(HrBusinessErrorCodes.ReportSortDirectionInvalid);
        RuleFor(x => x.Status)
            .Must(x => string.IsNullOrWhiteSpace(x) || Enum.TryParse<PayslipStatus>(x, true, out _))
            .WithMessage(HrBusinessErrorCodes.ReportStatusInvalid);
        RuleFor(x => x.GeneratedTo)
            .GreaterThanOrEqualTo(x => x.GeneratedFrom)
            .When(x => x.GeneratedFrom.HasValue && x.GeneratedTo.HasValue)
            .WithMessage(HrBusinessErrorCodes.ReportDateRangeInvalid);
    }
}
