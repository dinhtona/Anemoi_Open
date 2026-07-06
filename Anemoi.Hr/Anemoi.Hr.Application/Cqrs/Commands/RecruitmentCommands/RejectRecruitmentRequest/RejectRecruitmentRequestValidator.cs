using Anemoi.Hr.Application.Configurations;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.RejectRecruitmentRequest;

public sealed class RejectRecruitmentRequestValidator : AbstractValidator<RejectRecruitmentRequestCommand>
{
    public RejectRecruitmentRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithErrorCode(HrBusinessErrorCodes.ValRecruitmentRequestIdRequired);
    }
}
