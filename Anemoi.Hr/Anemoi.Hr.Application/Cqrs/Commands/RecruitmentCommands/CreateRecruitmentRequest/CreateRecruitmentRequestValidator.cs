using Anemoi.Hr.Application.Configurations;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.CreateRecruitmentRequest;

public sealed class CreateRecruitmentRequestValidator : AbstractValidator<CreateRecruitmentRequestCommand>
{
    public CreateRecruitmentRequestValidator()
    {
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
