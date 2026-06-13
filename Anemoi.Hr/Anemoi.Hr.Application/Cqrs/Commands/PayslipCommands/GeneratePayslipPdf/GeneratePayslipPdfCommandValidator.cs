using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.PayslipCommands.GeneratePayslipPdf;

public sealed class GeneratePayslipPdfCommandValidator : AbstractValidator<GeneratePayslipPdfCommand>
{
    public GeneratePayslipPdfCommandValidator()
    {
        RuleFor(x => x.PayslipId)
            .NotEmpty()
            .WithMessage("PayslipId must not be empty.");
    }
}
