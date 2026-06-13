using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.EssCommands.CancelMyOvertimeRequest;

public sealed class CancelMyOvertimeRequestValidator : AbstractValidator<CancelMyOvertimeRequestCommand>
{
    public CancelMyOvertimeRequestValidator()
    {
        RuleFor(x => x.OvertimeRequestId).RequiredId("VAL_OVERTIME_REQUEST_ID_REQUIRED");
    }
}
