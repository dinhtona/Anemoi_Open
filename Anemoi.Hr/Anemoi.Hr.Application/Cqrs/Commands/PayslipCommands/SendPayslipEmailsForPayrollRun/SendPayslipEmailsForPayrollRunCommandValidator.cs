using Anemoi.Hr.Application.Configurations;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.PayslipCommands.SendPayslipEmailsForPayrollRun;

public sealed class SendPayslipEmailsForPayrollRunCommandValidator : AbstractValidator<SendPayslipEmailsForPayrollRunCommand>
{
    public SendPayslipEmailsForPayrollRunCommandValidator()
    {
        RuleFor(x => x.PayrollRunId)
            .NotEmpty()
            .WithMessage(HrBusinessErrorCodes.ValPayrollRunIdRequired);
    }
}
