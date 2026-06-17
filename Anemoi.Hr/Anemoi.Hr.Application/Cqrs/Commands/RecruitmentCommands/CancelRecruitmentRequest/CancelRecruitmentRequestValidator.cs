using Anemoi.Hr.Application.Configurations;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.CancelRecruitmentRequest;

public sealed class CancelRecruitmentRequestValidator : AbstractValidator<CancelRecruitmentRequestCommand>
{
    public CancelRecruitmentRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithErrorCode(HrBusinessErrorCodes.ValRecruitmentRequestIdRequired);
    }
}
