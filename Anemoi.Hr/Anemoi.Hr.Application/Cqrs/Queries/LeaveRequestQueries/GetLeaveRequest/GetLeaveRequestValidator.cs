using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Queries.LeaveRequestQueries.GetLeaveRequest;

public sealed class GetLeaveRequestValidator : AbstractValidator<GetLeaveRequestQuery>
{
    public GetLeaveRequestValidator()
    {
        RuleFor(x => x.Id).RequiredId("VAL_LEAVE_REQUEST_ID_REQUIRED");
    }
}
