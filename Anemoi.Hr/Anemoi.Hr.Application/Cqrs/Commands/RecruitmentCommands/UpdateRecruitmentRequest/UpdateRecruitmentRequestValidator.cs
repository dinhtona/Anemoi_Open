using Anemoi.Hr.Application.Configurations;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.UpdateRecruitmentRequest;

public sealed class UpdateRecruitmentRequestValidator : AbstractValidator<UpdateRecruitmentRequestCommand>
{
    public UpdateRecruitmentRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithErrorCode(HrBusinessErrorCodes.ValRecruitmentRequestIdRequired);
        RuleFor(x => x.DepartmentId)
            .NotEmpty().WithErrorCode(HrBusinessErrorCodes.ValRecruitmentRequestDepartmentRequired);
        RuleFor(x => x.PositionId)
            .NotEmpty().WithErrorCode(HrBusinessErrorCodes.ValRecruitmentRequestPositionRequired);
        RuleFor(x => x.RequestedHeadcount)
            .GreaterThan(0).WithErrorCode(HrBusinessErrorCodes.ValRecruitmentRequestHeadcountPositive);
        RuleFor(x => x.PriorityCode)
            .NotEmpty().WithErrorCode(HrBusinessErrorCodes.ValRecruitmentRequestPriorityRequired);
    }
}
