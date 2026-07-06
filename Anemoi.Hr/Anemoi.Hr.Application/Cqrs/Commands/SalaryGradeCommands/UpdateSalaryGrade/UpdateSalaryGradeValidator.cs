using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.SalaryGradeCommands.UpdateSalaryGrade;

public sealed class UpdateSalaryGradeValidator : AbstractValidator<UpdateSalaryGradeCommand>
{
    public UpdateSalaryGradeValidator()
    {
        RuleFor(x => x.Id).RequiredId(HrBusinessErrorCodes.ValSalaryGradeIdRequired);
        RuleFor(x => x.GradeCode).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(128);
        RuleFor(x => x.Description).MaximumLength(512);
    }
}
