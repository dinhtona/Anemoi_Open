using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Queries.OrganizationQueries.GetApproversPreview;

public sealed class GetApproversPreviewValidator : AbstractValidator<GetApproversPreviewQuery>
{
    public GetApproversPreviewValidator()
    {
        RuleFor(x => x.EmployeeId).RequiredId(HrBusinessErrorCodes.ValEmployeeIdRequired);
        RuleFor(x => x.EntityType).NotEmpty().WithErrorCode(HrBusinessErrorCodes.ValEntityTypeRequired);
    }
}
