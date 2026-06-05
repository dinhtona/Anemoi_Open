using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.EmployeeCommands.LinkEmployeesToIdentityUsers;

public sealed class LinkEmployeesToIdentityUsersValidator : AbstractValidator<LinkEmployeesToIdentityUsersCommand>
{
    public LinkEmployeesToIdentityUsersValidator()
    {
        RuleFor(x => x.Candidates).NotNull();
        RuleForEach(x => x.Candidates).ChildRules(candidate =>
        {
            candidate.RuleFor(x => x.IdentityUserId).NotEmpty();
            candidate.RuleFor(x => x.Email).MaximumLength(256);
        });
    }
}
