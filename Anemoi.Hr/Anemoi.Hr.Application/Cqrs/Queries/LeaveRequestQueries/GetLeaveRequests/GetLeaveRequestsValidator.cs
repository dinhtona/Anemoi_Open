using Anemoi.BuildingBlock.Application.Validations.GetManyValidations;
using Anemoi.Hr.Application.Configurations;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Queries.LeaveRequestQueries.GetLeaveRequests;

public sealed class GetLeaveRequestsValidator : GetManyValidator<GetLeaveRequestsQuery>
{
    public GetLeaveRequestsValidator()
    {
        When(x => x.FromDate is { } && x.ToDate is { }, () =>
            RuleFor(x => x.ToDate)
                .GreaterThanOrEqualTo(x => x.FromDate)
                .WithMessage(HrBusinessErrorCodes.LeaveRequestInvalidDateRange));
    }
}
