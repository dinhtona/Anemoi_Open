using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.EmployeeCommands.CreateEmployee;

public sealed class CreateEmployeeValidator : AbstractValidator<CreateEmployeeCommand>
{
    public CreateEmployeeValidator()
    {
        RuleFor(x => x.EmployeeCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.WorkEmail).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(20);
        RuleFor(x => x.JoinDate).NotEmpty();
        RuleFor(x => x.EmploymentTypeCode).NotEmpty();
        RuleFor(x => x.PrimaryDepartmentId).RequiredId(HrBusinessErrorCodes.ValDepartmentIdRequired);
        RuleFor(x => x.PrimaryPositionId).RequiredId(HrBusinessErrorCodes.ValPositionIdRequired);
    }
}
