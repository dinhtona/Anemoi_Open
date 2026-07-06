using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.EmployeeCommands.TransferEmployee;

public sealed class TransferEmployeeValidator : AbstractValidator<TransferEmployeeCommand>
{
    public TransferEmployeeValidator()
    {
        RuleFor(x => x.EmployeeId).RequiredId(HrBusinessErrorCodes.ValEmployeeIdRequired);
        RuleFor(x => x.NewDepartmentId).RequiredId(HrBusinessErrorCodes.ValNewDepartmentIdRequired);
        RuleFor(x => x.ReasonCode).NotEmpty().MaximumLength(64);
        RuleFor(x => x.EffectiveDate).NotEmpty();
    }
}
