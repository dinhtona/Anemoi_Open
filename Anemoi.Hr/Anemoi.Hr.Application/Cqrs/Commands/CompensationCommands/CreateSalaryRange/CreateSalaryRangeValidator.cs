using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.CompensationCommands.CreateSalaryRange;

public sealed class CreateSalaryRangeValidator : AbstractValidator<CreateSalaryRangeCommand>
{
    public CreateSalaryRangeValidator()
    {
        RuleFor(x => x.SalaryGradeId).RequiredId(HrBusinessErrorCodes.ValSalaryGradeIdRequired);
        RuleFor(x => x.MinSalary).GreaterThan(0).WithMessage(HrBusinessErrorCodes.ValMinSalaryMustBePositive);
        RuleFor(x => x.MaxSalary).GreaterThanOrEqualTo(x => x.MinSalary).WithMessage(HrBusinessErrorCodes.ValMaxSalaryMustBeGeMinSalary);
        RuleFor(x => x.Currency).NotEmpty().MaximumLength(16);
        RuleFor(x => x.EffectiveFrom).NotEmpty();
        
        RuleFor(x => x.SensitivePermissionConfirmed)
            .Equal(true)
            .WithMessage(HrBusinessErrorCodes.PermissionSensitiveConfirmationRequired);
    }
}
