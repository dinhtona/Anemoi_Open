using Anemoi.Hr.Application.Configurations;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.PayslipCommands.GeneratePayslipPdf;

public sealed class GeneratePayslipPdfCommandValidator : AbstractValidator<GeneratePayslipPdfCommand>
{
    public GeneratePayslipPdfCommandValidator()
    {
        RuleFor(x => x.PayslipId)
            .NotEmpty()
            .WithMessage(HrBusinessErrorCodes.PayslipIdRequired);
    }
}
