using Anemoi.Hr.Application.Configurations;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Queries.PayslipQueries.GetPayslipEmailDeliveries;

public sealed class GetPayslipEmailDeliveriesQueryValidator : AbstractValidator<GetPayslipEmailDeliveriesQuery>
{
    public GetPayslipEmailDeliveriesQueryValidator()
    {
        RuleFor(x => x.PayslipId)
            .NotEmpty()
            .WithMessage(HrBusinessErrorCodes.PayslipIdRequired);
    }
}
