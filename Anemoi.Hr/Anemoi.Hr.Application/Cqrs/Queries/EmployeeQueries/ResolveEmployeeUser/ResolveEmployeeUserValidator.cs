using Anemoi.Contract.Hr.Queries;
using Anemoi.Hr.Application.Configurations;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Queries.EmployeeQueries.ResolveEmployeeUser;

public sealed class ResolveEmployeeUserValidator : AbstractValidator<ResolveEmployeeUserQuery>
{
    public ResolveEmployeeUserValidator()
    {
        RuleFor(x => x.EmployeeId)
            .NotEmpty()
            .WithErrorCode(HrBusinessErrorCodes.ValEmployeeIdRequired)
            .Must(id => Guid.TryParse(id, out _))
            .WithErrorCode(HrBusinessErrorCodes.ValEmployeeIdInvalidGuid);
    }
}
