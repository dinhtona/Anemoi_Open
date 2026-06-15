#nullable enable

using System;
using Anemoi.Hr.Application.Configurations;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Queries.PayrollReportingQueries.GetPayrollVarianceReport;

public sealed class GetPayrollVarianceReportQueryValidator : AbstractValidator<GetPayrollVarianceReportQuery>
{
    public GetPayrollVarianceReportQueryValidator()
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

        RuleFor(x => x)
            .Must(x => x.CurrentPayrollPeriodId != null || x.CurrentPayrollRunId != null)
            .WithMessage(HrBusinessErrorCodes.ReportVarianceCurrentCriteriaRequired)
            .Must(x => x.PreviousPayrollPeriodId != null || x.PreviousPayrollRunId != null)
            .WithMessage(HrBusinessErrorCodes.ReportVariancePreviousCriteriaRequired);
    }
}
