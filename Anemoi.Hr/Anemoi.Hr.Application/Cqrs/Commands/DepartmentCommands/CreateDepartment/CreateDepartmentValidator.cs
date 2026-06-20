using Anemoi.Hr.Application.Configurations;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.DepartmentCommands.CreateDepartment;

public sealed class CreateDepartmentValidator : AbstractValidator<CreateDepartmentCommand>
{
    public CreateDepartmentValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        RuleFor(x => x.DepartmentTypeCode)
            .NotEmpty()
            .WithMessage(HrBusinessErrorCodes.ValDepartmentTypeCodeInvalid);
    }
}
