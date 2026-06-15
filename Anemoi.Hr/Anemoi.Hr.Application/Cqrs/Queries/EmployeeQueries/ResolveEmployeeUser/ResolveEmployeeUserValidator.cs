using Anemoi.Contract.Hr.Queries;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Queries.EmployeeQueries.ResolveEmployeeUser;

public sealed class ResolveEmployeeUserValidator : AbstractValidator<ResolveEmployeeUserQuery>
{
    public ResolveEmployeeUserValidator()
    {
        RuleFor(x => x.EmployeeId)
            .NotEmpty()
            .Must(id => Guid.TryParse(id, out _))
            .WithMessage("EmployeeId must be a valid Guid.");
    }
}
