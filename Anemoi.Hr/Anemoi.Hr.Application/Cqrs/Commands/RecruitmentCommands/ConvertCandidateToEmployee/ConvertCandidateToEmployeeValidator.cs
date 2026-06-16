using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.ConvertCandidateToEmployee;

public sealed class ConvertCandidateToEmployeeValidator : AbstractValidator<ConvertCandidateToEmployeeCommand>
{
    public ConvertCandidateToEmployeeValidator()
    {
        RuleFor(x => x.CandidateId)
            .RequiredId(HrBusinessErrorCodes.ValCandidateIdRequired);
        RuleFor(x => x.EmployeeCode)
            .NotEmpty().WithMessage(HrBusinessErrorCodes.ValRequisitionCodeRequired);
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage(HrBusinessErrorCodes.ValCandidateNameRequired);
        RuleFor(x => x.DepartmentId)
            .RequiredId(HrBusinessErrorCodes.ValRequisitionIdRequired);
        RuleFor(x => x.PositionId)
            .RequiredId(HrBusinessErrorCodes.ValRequisitionIdRequired);
        RuleFor(x => x.JoinDate)
            .NotEmpty().WithMessage(HrBusinessErrorCodes.ValStartDateRequired);
        RuleFor(x => x.EmploymentTypeCode)
            .NotEmpty().WithMessage(HrBusinessErrorCodes.ValRequisitionEmploymentTypeRequired);
    }
}
