using Anemoi.Hr.Application.Configurations;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.PositionCommands.CreatePosition;

public sealed class CreatePositionValidator : AbstractValidator<CreatePositionCommand>
{
    public CreatePositionValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        RuleFor(x => x.PositionTypeCode)
            .NotEmpty()
            .WithMessage(HrBusinessErrorCodes.ValPositionTypeCodeInvalid);
    }
}
