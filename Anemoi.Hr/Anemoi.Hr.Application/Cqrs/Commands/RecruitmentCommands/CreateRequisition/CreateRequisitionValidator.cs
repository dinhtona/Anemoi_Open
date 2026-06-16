using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.CreateRequisition;

public sealed class CreateRequisitionValidator : AbstractValidator<CreateRequisitionCommand>
{
    public CreateRequisitionValidator()
    {
        RuleFor(x => x.RequisitionCode)
            .NotEmpty().WithMessage(HrBusinessErrorCodes.ValRequisitionCodeRequired)
            .MaximumLength(64).WithMessage(HrBusinessErrorCodes.ValRequisitionCodeTooLong);
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
        RuleFor(x => x.OpenDate)
            .NotEmpty().WithMessage(HrBusinessErrorCodes.ValRequisitionOpenDateRequired);
        RuleFor(x => x.TargetHireDate)
            .NotEmpty().WithMessage(HrBusinessErrorCodes.ValRequisitionTargetHireDateRequired);
        RuleFor(x => x)
            .Must(x => x.TargetHireDate >= x.OpenDate)
            .WithMessage(HrBusinessErrorCodes.RequisitionDateRangeInvalid);
    }
}
