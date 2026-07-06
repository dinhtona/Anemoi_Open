using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Queries.LeavePolicyQueries.GetLeavePolicy;

public sealed class GetLeavePolicyValidator : AbstractValidator<GetLeavePolicyQuery>
{
    public GetLeavePolicyValidator()
    {
        RuleFor(x => x.Id).RequiredId(HrBusinessErrorCodes.ValLeavePolicyIdRequired);
    }
}
