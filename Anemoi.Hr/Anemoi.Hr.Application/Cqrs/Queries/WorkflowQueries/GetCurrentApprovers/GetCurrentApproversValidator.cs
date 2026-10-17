using System;
using Anemoi.Hr.Application.Configurations;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Queries.WorkflowQueries.GetCurrentApprovers;

public sealed class GetCurrentApproversValidator : AbstractValidator<GetCurrentApproversQuery>
{
    public GetCurrentApproversValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithErrorCode(HrBusinessErrorCodes.ValWorkflowInstanceIdRequired)
            .Must(id => Guid.TryParse(id, out _))
            .WithErrorCode(HrBusinessErrorCodes.ValWorkflowInstanceIdInvalidGuid);
    }
}
