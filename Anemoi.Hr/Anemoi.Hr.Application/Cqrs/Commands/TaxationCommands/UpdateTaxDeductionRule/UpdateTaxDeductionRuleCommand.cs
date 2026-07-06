using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Commands.TaxationCommands.UpdateTaxDeductionRule;

public sealed record UpdateTaxDeductionRuleCommand(
    string Id,
    decimal Amount,
    bool IsActive) : ICommandResult<SuccessResponse>;
