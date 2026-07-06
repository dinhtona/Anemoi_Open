using Anemoi.Hr.Application.Configurations;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.PayslipCommands.GeneratePayslipPdfsForPayrollRun;

public sealed class GeneratePayslipPdfsForPayrollRunCommandValidator : AbstractValidator<GeneratePayslipPdfsForPayrollRunCommand>
{
    public GeneratePayslipPdfsForPayrollRunCommandValidator()
    {
        RuleFor(x => x.PayrollRunId)
            .NotEmpty()
            .WithMessage(HrBusinessErrorCodes.ValPayrollRunIdRequired);
    }
}
