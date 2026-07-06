using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.ContractCommands.UpdateContract;

public sealed class UpdateContractValidator : AbstractValidator<UpdateContractCommand>
{
    public UpdateContractValidator()
    {
        RuleFor(x => x.Id).RequiredId(HrBusinessErrorCodes.ValContractIdRequired);
        RuleFor(x => x.ContractNumber).NotEmpty().MaximumLength(64);
        RuleFor(x => x.ContractTypeCode).NotEmpty().MaximumLength(64);
        RuleFor(x => x.StartDate).NotEmpty();
        RuleFor(x => x.SignedDate).NotEmpty();
        RuleFor(x => x.Notes).MaximumLength(1024);
        RuleFor(x => x.AttachmentFileId).MaximumLength(128);
    }
}
