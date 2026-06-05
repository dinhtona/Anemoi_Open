using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.ContractCommands.CreateContract;

public sealed class CreateContractValidator : AbstractValidator<CreateContractCommand>
{
    public CreateContractValidator()
    {
        RuleFor(x => x.EmployeeId).RequiredId("VAL_EMPLOYEE_ID_REQUIRED");
        RuleFor(x => x.ContractNumber).NotEmpty().MaximumLength(64);
        RuleFor(x => x.ContractTypeCode).NotEmpty().MaximumLength(64);
        RuleFor(x => x.StartDate).NotEmpty();
        RuleFor(x => x.SignedDate).NotEmpty();
        RuleFor(x => x.Notes).MaximumLength(1024);
        RuleFor(x => x.AttachmentFileId).MaximumLength(128);
    }
}
