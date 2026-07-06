using FluentValidation;

namespace Anemoi.Contract.Identity.Commands.IdentityCommands.RemoveUsers;

public sealed class RemoveUsersValidator : AbstractValidator<RemoveUsersCommand>
{
    public RemoveUsersValidator()
    {
        RuleFor(x => x.UserIds)
            .NotEmpty()
            .Must(x => x.All(id => id is { } && id.Value != Guid.Empty))
            .WithMessage("VAL_IDS_NOT_EMPTY")
            .Must(x => x.Distinct().Count() == x.Count)
            .WithMessage("VAL_IDS_NOT_DUPLICATED");
    }
}