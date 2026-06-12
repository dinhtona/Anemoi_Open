using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using Anemoi.Hr.ModelIds.ModelIds;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Queries.OvertimeRequestQueries.GetOvertimeRequests;

public sealed class GetOvertimeRequestsValidator : AbstractValidator<GetOvertimeRequestsQuery>
{
    public GetOvertimeRequestsValidator()
    {
        RuleFor(x => x.EmployeeId)
            .NotNull()
            .WithErrorCode(HrBusinessErrorCodes.EmployeeNotFound);

        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithErrorCode(HrBusinessErrorCodes.ReportPageInvalid);

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithErrorCode(HrBusinessErrorCodes.ReportPageSizeInvalid);

        RuleFor(x => x.SortDirection)
            .Must(x => x.ToLower() == "asc" || x.ToLower() == "desc")
            .WithErrorCode(HrBusinessErrorCodes.ReportSortDirectionInvalid);
    }
}
