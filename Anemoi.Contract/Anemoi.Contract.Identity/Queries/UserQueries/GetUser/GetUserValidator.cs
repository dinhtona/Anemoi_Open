using FluentValidation;

namespace Anemoi.Contract.Identity.Queries.UserQueries.GetUser;

public sealed class GetUserValidator : AbstractValidator<GetUserQuery>
{
    public GetUserValidator()
    {
        RuleFor(x => x.Id)
            .NotNull()
            .Must(x => x is { } && x.Value != Guid.Empty)
            .WithMessage("VAL_USER_ID_REQUIRED");
    }
}