using FluentValidation;

namespace Anemoi.Contract.Identity.Commands.IdentityCommands.LockUser;

public sealed class LockUserValidator : AbstractValidator<LockUserCommand>
{
    public LockUserValidator()
    {
        RuleFor(x => x.LockUntil)
            .Must((lockCommand, time) => !lockCommand.EnableLock || time is { } && time > DateTime.UtcNow)
            .WithMessage("VAL_LOCK_UNTIL_FUTURE");
    }
}