using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.PositionCommands.UpdatePosition;

public sealed class UpdatePositionValidator : AbstractValidator<UpdatePositionCommand>
{
    public UpdatePositionValidator()
    {
        RuleFor(x => x.Id).RequiredId(HrBusinessErrorCodes.ValPositionIdRequired);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        RuleFor(x => x.PositionTypeCode)
            .NotEmpty()
            .WithMessage(HrBusinessErrorCodes.ValPositionTypeCodeInvalid);
    }
}
