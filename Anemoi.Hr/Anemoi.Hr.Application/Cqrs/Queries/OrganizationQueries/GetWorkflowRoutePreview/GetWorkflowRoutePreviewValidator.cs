using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Queries.OrganizationQueries.GetWorkflowRoutePreview;

public sealed class GetWorkflowRoutePreviewValidator : AbstractValidator<GetWorkflowRoutePreviewQuery>
{
    public GetWorkflowRoutePreviewValidator()
    {
        RuleFor(x => x.EmployeeId).RequiredId(HrBusinessErrorCodes.ValEmployeeIdRequired);
        RuleFor(x => x.EntityType).NotEmpty().WithErrorCode(HrBusinessErrorCodes.ValEntityTypeRequired);
    }
}
