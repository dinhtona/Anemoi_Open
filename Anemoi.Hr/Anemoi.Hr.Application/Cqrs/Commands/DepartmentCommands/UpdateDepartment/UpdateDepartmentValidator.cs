using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.DepartmentCommands.UpdateDepartment;

public sealed class UpdateDepartmentValidator : AbstractValidator<UpdateDepartmentCommand>
{
    public UpdateDepartmentValidator()
    {
        RuleFor(x => x.Id).RequiredId(HrBusinessErrorCodes.ValNewDepartmentIdRequired);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        RuleFor(x => x.DepartmentTypeCode)
            .NotEmpty()
            .WithMessage(HrBusinessErrorCodes.ValDepartmentTypeCodeInvalid);
    }
}
