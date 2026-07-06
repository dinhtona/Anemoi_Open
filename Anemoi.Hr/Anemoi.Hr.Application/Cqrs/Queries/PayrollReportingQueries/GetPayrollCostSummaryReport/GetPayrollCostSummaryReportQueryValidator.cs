#nullable enable

using System;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Domain.Payroll;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Queries.PayrollReportingQueries.GetPayrollCostSummaryReport;

public sealed class GetPayrollCostSummaryReportQueryValidator : AbstractValidator<GetPayrollCostSummaryReportQuery>
{
    public GetPayrollCostSummaryReportQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage(HrBusinessErrorCodes.ReportPageInvalid);
        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 1000)
            .WithMessage(HrBusinessErrorCodes.ReportPageSizeInvalid);
        RuleFor(x => x.SortDirection)
            .Must(x => string.IsNullOrWhiteSpace(x)
                || x.Equals(SortDirectionConstants.Asc, StringComparison.OrdinalIgnoreCase)
                || x.Equals(SortDirectionConstants.Desc, StringComparison.OrdinalIgnoreCase)
                || x.Equals(SortDirectionConstants.Ascending, StringComparison.OrdinalIgnoreCase)
                || x.Equals(SortDirectionConstants.Descending, StringComparison.OrdinalIgnoreCase))
            .WithMessage(HrBusinessErrorCodes.ReportSortDirectionInvalid);
        RuleFor(x => x.Status)
            .Must(x => string.IsNullOrWhiteSpace(x) || Enum.TryParse<PayrollRunStatus>(x, true, out _))
            .WithMessage(HrBusinessErrorCodes.ReportStatusInvalid);
        RuleFor(x => x.ToDate)
            .GreaterThanOrEqualTo(x => x.FromDate)
            .When(x => x.FromDate.HasValue && x.ToDate.HasValue)
            .WithMessage(HrBusinessErrorCodes.ReportDateRangeInvalid);
    }
}
