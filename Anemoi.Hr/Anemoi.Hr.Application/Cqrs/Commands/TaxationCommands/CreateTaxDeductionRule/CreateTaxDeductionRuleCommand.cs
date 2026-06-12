using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Commands.TaxationCommands.CreateTaxDeductionRule;

public sealed record CreateTaxDeductionRuleCommand(
    string TaxRuleSetId,
    string DeductionType,
    decimal Amount,
    bool IsActive) : ICommandResult<CreateTaxDeductionRuleResponse>;
