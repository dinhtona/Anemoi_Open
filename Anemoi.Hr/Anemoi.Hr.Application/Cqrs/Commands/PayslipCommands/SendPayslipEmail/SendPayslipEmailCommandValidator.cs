using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.PayslipCommands.SendPayslipEmail;

public sealed class SendPayslipEmailCommandValidator : AbstractValidator<SendPayslipEmailCommand>
{
    public SendPayslipEmailCommandValidator()
    {
        RuleFor(x => x.PayslipId)
            .NotEmpty()
            .WithMessage("PayslipId must not be empty.");
    }
}
