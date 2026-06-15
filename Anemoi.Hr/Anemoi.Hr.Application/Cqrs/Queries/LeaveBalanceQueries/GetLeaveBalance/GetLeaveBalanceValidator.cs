using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Queries.LeaveBalanceQueries.GetLeaveBalance;

public sealed class GetLeaveBalanceValidator : AbstractValidator<GetLeaveBalanceQuery>
{
    public GetLeaveBalanceValidator()
    {
        RuleFor(x => x.Id).RequiredId(HrBusinessErrorCodes.ValLeaveBalanceIdRequired);
    }
}
