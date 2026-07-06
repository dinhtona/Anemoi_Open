#nullable enable

using System;
using Anemoi.Hr.Application.Configurations;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Queries.PayrollReportingQueries.ExportPayslipDeliveryCsv;

public sealed class ExportPayslipDeliveryCsvQueryValidator : AbstractValidator<ExportPayslipDeliveryCsvQuery>
{
    public ExportPayslipDeliveryCsvQueryValidator()
    {
        RuleFor(x => x.SortDirection)
            .Must(x => string.IsNullOrWhiteSpace(x)
                || x.Equals(SortDirectionConstants.Asc, StringComparison.OrdinalIgnoreCase)
                || x.Equals(SortDirectionConstants.Desc, StringComparison.OrdinalIgnoreCase)
                || x.Equals(SortDirectionConstants.Ascending, StringComparison.OrdinalIgnoreCase)
                || x.Equals(SortDirectionConstants.Descending, StringComparison.OrdinalIgnoreCase))
            .WithMessage(HrBusinessErrorCodes.ReportSortDirectionInvalid);
    }
}
