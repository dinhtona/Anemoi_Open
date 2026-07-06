using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.UpdateRequisition;

public sealed class UpdateRequisitionValidator : AbstractValidator<UpdateRequisitionCommand>
{
    public UpdateRequisitionValidator()
    {
        RuleFor(x => x.Id)
            .RequiredId(HrBusinessErrorCodes.ValRequisitionIdRequired);
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage(HrBusinessErrorCodes.ValRequisitionTitleRequired);
        RuleFor(x => x.DepartmentId)
            .RequiredId(HrBusinessErrorCodes.ValRequisitionIdRequired);
        RuleFor(x => x.PositionId)
            .RequiredId(HrBusinessErrorCodes.ValRequisitionIdRequired);
        RuleFor(x => x.Headcount)
            .GreaterThan(0).WithMessage(HrBusinessErrorCodes.ValRequisitionHeadcountPositive);
        RuleFor(x => x.EmploymentType)
            .NotEmpty().WithMessage(HrBusinessErrorCodes.ValRequisitionEmploymentTypeRequired);
        RuleFor(x => x)
            .Must(x => x.TargetHireDate >= x.OpenDate)
            .WithMessage(HrBusinessErrorCodes.RequisitionDateRangeInvalid);
    }
}
