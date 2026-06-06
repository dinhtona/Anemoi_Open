using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.CompensationCommands.CreateSalaryRange;

public sealed class CreateSalaryRangeValidator : AbstractValidator<CreateSalaryRangeCommand>
{
    public CreateSalaryRangeValidator()
    {
        RuleFor(x => x.SalaryGradeId).RequiredId("VAL_SALARY_GRADE_ID_REQUIRED");
        RuleFor(x => x.MinSalary).GreaterThan(0).WithMessage("VAL_MIN_SALARY_MUST_BE_POSITIVE");
        RuleFor(x => x.MaxSalary).GreaterThanOrEqualTo(x => x.MinSalary).WithMessage("VAL_MAX_SALARY_MUST_BE_GE_MIN_SALARY");
        RuleFor(x => x.Currency).NotEmpty().MaximumLength(16);
        RuleFor(x => x.EffectiveFrom).NotEmpty();
        
        RuleFor(x => x.SensitivePermissionConfirmed)
            .Equal(true)
            .WithMessage(HrBusinessErrorCodes.PermissionSensitiveConfirmationRequired);
    }
}
