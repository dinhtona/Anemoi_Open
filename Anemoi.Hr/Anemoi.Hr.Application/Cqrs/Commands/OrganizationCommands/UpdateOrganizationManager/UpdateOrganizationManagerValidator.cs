using System;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.OrganizationCommands.UpdateOrganizationManager;

public sealed class UpdateOrganizationManagerValidator : AbstractValidator<UpdateOrganizationManagerCommand>
{
    public UpdateOrganizationManagerValidator()
    {
        RuleFor(x => x.EmployeeId).RequiredId(HrBusinessErrorCodes.ValEmployeeIdRequired);
        RuleFor(x => x.ManagerId)
            .Must(id => id is null || id.Value != Guid.Empty)
            .WithErrorCode(HrBusinessErrorCodes.ValEmployeeIdRequired);
    }
}
