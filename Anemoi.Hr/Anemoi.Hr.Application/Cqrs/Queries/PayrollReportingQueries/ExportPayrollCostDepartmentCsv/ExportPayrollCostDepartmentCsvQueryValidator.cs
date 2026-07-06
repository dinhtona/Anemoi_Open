#nullable enable

using System;
using Anemoi.Hr.Application.Configurations;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Queries.PayrollReportingQueries.ExportPayrollCostDepartmentCsv;

public sealed class ExportPayrollCostDepartmentCsvQueryValidator : AbstractValidator<ExportPayrollCostDepartmentCsvQuery>
{
    public ExportPayrollCostDepartmentCsvQueryValidator()
    {
        RuleFor(x => x.SortDirection)
            .Must(x => string.IsNullOrWhiteSpace(x)
                || x.Equals(SortDirectionConstants.Asc, StringComparison.OrdinalIgnoreCase)
                || x.Equals(SortDirectionConstants.Desc, StringComparison.OrdinalIgnoreCase)
                || x.Equals(SortDirectionConstants.Ascending, StringComparison.OrdinalIgnoreCase)
                || x.Equals(SortDirectionConstants.Descending, StringComparison.OrdinalIgnoreCase))
            .WithMessage(HrBusinessErrorCodes.ReportSortDirectionInvalid);
        RuleFor(x => x.ToDate)
            .GreaterThanOrEqualTo(x => x.FromDate)
            .When(x => x.FromDate.HasValue && x.ToDate.HasValue)
            .WithMessage(HrBusinessErrorCodes.ReportDateRangeInvalid);
    }
}
