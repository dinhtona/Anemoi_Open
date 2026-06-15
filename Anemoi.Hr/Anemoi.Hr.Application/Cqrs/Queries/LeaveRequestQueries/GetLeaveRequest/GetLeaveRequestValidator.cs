using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Queries.LeaveRequestQueries.GetLeaveRequest;

public sealed class GetLeaveRequestValidator : AbstractValidator<GetLeaveRequestQuery>
{
    public GetLeaveRequestValidator()
    {
        RuleFor(x => x.Id).RequiredId(HrBusinessErrorCodes.ValLeaveRequestIdRequired);
    }
}
