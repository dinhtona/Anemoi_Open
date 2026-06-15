#nullable enable

using System;
using Anemoi.Hr.Application.Configurations;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Queries.PayrollReportingQueries.ExportPayrollVarianceCsv;

public sealed class ExportPayrollVarianceCsvQueryValidator : AbstractValidator<ExportPayrollVarianceCsvQuery>
{
    public ExportPayrollVarianceCsvQueryValidator()
    {
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
