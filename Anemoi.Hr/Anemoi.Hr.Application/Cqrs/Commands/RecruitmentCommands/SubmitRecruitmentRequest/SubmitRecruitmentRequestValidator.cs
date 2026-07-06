using Anemoi.Hr.Application.Configurations;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.SubmitRecruitmentRequest;

public sealed class SubmitRecruitmentRequestValidator : AbstractValidator<SubmitRecruitmentRequestCommand>
{
    public SubmitRecruitmentRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithErrorCode(HrBusinessErrorCodes.ValRecruitmentRequestIdRequired);
    }
}
