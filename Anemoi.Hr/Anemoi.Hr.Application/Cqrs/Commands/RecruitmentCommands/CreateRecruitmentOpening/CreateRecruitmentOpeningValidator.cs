using Anemoi.Hr.Application.Configurations;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.CreateRecruitmentOpening;

public sealed class CreateRecruitmentOpeningValidator : AbstractValidator<CreateRecruitmentOpeningCommand>
{
    public CreateRecruitmentOpeningValidator()
    {
        RuleFor(x => x.RecruitmentRequestId)
            .NotEmpty().WithErrorCode(HrBusinessErrorCodes.ValRecruitmentRequestIdRequired);
        RuleFor(x => x.Code)
            .NotEmpty().WithErrorCode("VAL_OPENING_CODE_REQUIRED");
        RuleFor(x => x.PlannedHeadcount)
            .GreaterThan(0).WithErrorCode(HrBusinessErrorCodes.ValRecruitmentRequestHeadcountPositive);
    }
}
