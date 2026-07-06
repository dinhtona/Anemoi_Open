using Anemoi.Hr.Application.Configurations;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.ApproveRecruitmentRequest;

public sealed class ApproveRecruitmentRequestValidator : AbstractValidator<ApproveRecruitmentRequestCommand>
{
    public ApproveRecruitmentRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithErrorCode(HrBusinessErrorCodes.ValRecruitmentRequestIdRequired);
    }
}
