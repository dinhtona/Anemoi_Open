using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.EmployeeCommands.TransferEmployee;

public sealed class TransferEmployeeValidator : AbstractValidator<TransferEmployeeCommand>
{
    public TransferEmployeeValidator()
    {
        RuleFor(x => x.EmployeeId).RequiredId("VAL_EMPLOYEE_ID_REQUIRED");
        RuleFor(x => x.NewDepartmentId).RequiredId("VAL_NEW_DEPARTMENT_ID_REQUIRED");
        RuleFor(x => x.ReasonCode).NotEmpty().MaximumLength(64);
        RuleFor(x => x.EffectiveDate).NotEmpty();
    }
}
