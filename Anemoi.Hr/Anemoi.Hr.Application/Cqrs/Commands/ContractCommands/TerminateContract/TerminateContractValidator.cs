using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.ContractCommands.TerminateContract;

public sealed class TerminateContractValidator : AbstractValidator<TerminateContractCommand>
{
    public TerminateContractValidator()
    {
        RuleFor(x => x.Id).RequiredId("VAL_CONTRACT_ID_REQUIRED");
        RuleFor(x => x.TerminationDate).NotEmpty();
        RuleFor(x => x.ReasonCode).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Notes).MaximumLength(1024);
        RuleFor(x => x.TerminationAttachmentId).MaximumLength(128);
    }
}
