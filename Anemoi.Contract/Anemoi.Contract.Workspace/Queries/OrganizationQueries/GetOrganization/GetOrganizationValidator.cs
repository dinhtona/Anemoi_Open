using FluentValidation;

namespace Anemoi.Contract.Workspace.Queries.OrganizationQueries.GetOrganization;

public sealed class GetOrganizationValidator : AbstractValidator<GetOrganizationQuery>
{
    public GetOrganizationValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("VAL_ORGANIZATION_ID_REQUIRED");
    }
}